"""Offline contract checks for RMS XML; this is not an RMS rendering test."""
import ast
import copy
import hashlib
from decimal import Decimal, ROUND_FLOOR, ROUND_HALF_UP
from pathlib import Path
import re
import unittest
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
RELATIVE = 'Receipts/Davao_Receipts/SOD-DAVAO_STORE_RECEIPT_WHOLESALE.xml'
XML = ET.parse(ROOT / RELATIVE).getroot()
SECTION = XML.find("SUB[@name='PrintWithholdingReference']")


def expression(text, values):
    # Interpret only the documented operators/functions used by this section.
    # No Python eval, SQL, COM, registry changes, or RMS session is involved.
    text = text.strip().replace('&', ' and ').replace('|', ' or ')
    text = re.sub(r'(?<![<>!=])=(?!=)', '==', text)

    def visit(node):
        if isinstance(node, ast.Constant):
            return Decimal(str(node.value)) if isinstance(node.value, (int, float)) else node.value
        if isinstance(node, (ast.Name, ast.Attribute)):
            return values[ast.unparse(node)]
        if isinstance(node, ast.BinOp):
            a, b = visit(node.left), visit(node.right)
            if isinstance(node.op, ast.Add): return a + b
            if isinstance(node.op, ast.Sub): return a - b
            if isinstance(node.op, ast.Mult): return a * b
            if isinstance(node.op, ast.Div): return a / b
        if isinstance(node, ast.BoolOp):
            items = [bool(visit(item)) for item in node.values]
            return all(items) if isinstance(node.op, ast.And) else any(items)
        if isinstance(node, ast.Compare) and len(node.ops) == 1:
            a, b = visit(node.left), visit(node.comparators[0])
            if isinstance(node.ops[0], ast.Eq): return a == b
            if isinstance(node.ops[0], ast.Gt): return a > b
            if isinstance(node.ops[0], ast.Lt): return a < b
        if isinstance(node, ast.Call) and len(node.args) == 1:
            value = visit(node.args[0])
            if ast.unparse(node.func) == 'Abs': return abs(value)
            if ast.unparse(node.func) == 'Int': return value.to_integral_value(rounding=ROUND_FLOOR)
        raise AssertionError('Unsupported receipt expression: ' + ast.dump(node))

    return visit(ast.parse(text, mode='eval').body)


def render_reference(total='224', vat='24', enabled=(1, 2, 5, 15), overrides=None):
    values = {'transactionWorkOrder': Decimal(2), 'transactionSales': Decimal(0), 'Transaction.Type': Decimal(2),
              'Customer.ID': Decimal(501), 'Transaction.Total': Decimal(total),
              'Transaction.SalesTax': Decimal(vat)}
    for rate, slot in ((1, 'Number3'), (2, 'Number4'), (5, 'Number5'), (15, 'Text2')):
        values['Customer.CustomCaption' + slot] = f'W/Tax {rate}%'
        values['Customer.Custom' + slot] = (str(rate) if rate in enabled else '') if rate == 15 else Decimal(rate if rate in enabled else 0)
    values.update(overrides or {})
    rows = []

    def walk(node):
        if node.tag == 'IF':
            branch = node.find('THEN' if expression(node.findtext('CONDITION'), values) else 'ELSE')
            if branch is not None:
                for child in branch: walk(child)
        elif node.tag == 'SET':
            values[node.attrib['name']] = expression(node.text, values)
        elif node.tag == 'ROW':
            tokens = re.findall(r'"[^"]*"|[A-Za-z][A-Za-z0-9.]*', node.text or '')
            rows.append([token[1:-1] if token.startswith('"') else values[token] for token in tokens])
        elif node.tag in ('SUB', 'TABLE'):
            for child in node: walk(child)
    walk(SECTION)
    return rows


def canonical(node):
    return (node.tag, sorted(node.attrib.items()), (node.text or '').strip(),
            tuple(canonical(child) for child in node))


class ReceiptChecks(unittest.TestCase):
    def test_original_receipt_elements_unchanged(self):
        candidate = copy.deepcopy(XML)
        candidate.remove(candidate.find("SUB[@name='PrintWithholdingReference']"))
        document = candidate.find('DOCUMENT')
        calls = list(document)
        self.assertEqual([call.text.strip() for call in calls],
                         ['"PrintEntries"', '"PrintWithholdingReference"', '"PrintAuthorize"'])
        document.remove(calls[1])
        # Canonical hash of the original receipt before this addition. Stable
        # across commits, indentation, line endings and XML comments.
        self.assertEqual(hashlib.sha256(repr(canonical(candidate)).encode()).hexdigest(),
                         '4b1945e5924d221fb64c22aa684bfc52652c673b093a4353b531b3e7f6b6154a')

    def test_all_rates_and_actual_vat_base(self):
        for total, vat in [('224', '24'), ('1100', '117.86'), ('1100', '0'),
                           ('0', '0'), ('-224', '-24'), ('0.5', '0'),
                           ('-0.5', '0'), ('99999999.99', '10714285.71')]:
            with self.subTest(total=total, vat=vat):
                rows = render_reference(total, vat)
                base = Decimal(total) - Decimal(vat)
                self.assertEqual(rows[0], ['VAT-exclusive:', '|', base])
                for rate, row in zip((1, 2, 5, 15), rows[1:]):
                    self.assertEqual(row, [f'WTax {rate}%', '|',
                        (base * Decimal(rate) / 100).quantize(Decimal('.01'), rounding=ROUND_HALF_UP)])
                self.assertEqual(len(rows), 5)

    def test_enabled_rates_compact_and_independent(self):
        for enabled in [(), (15,), (1, 15), (2, 5), (1,), (2,), (5,)]:
            with self.subTest(enabled=enabled):
                rows = render_reference(enabled=enabled)
                self.assertEqual([row[0] for row in rows],
                                 (['VAT-exclusive:'] if enabled else []) + [f'WTax {r}%' for r in enabled])

    def test_unrelated_captions_and_invalid_values_hidden(self):
        for slot, invalid in [('Number3', Decimal(2)), ('Number4', Decimal(1)),
                              ('Number5', Decimal(15)), ('Text2', '5')]:
            with self.subTest(slot=slot):
                expected = {'Number3': 1, 'Number4': 2, 'Number5': 5, 'Text2': 15}[slot]
                self.assertEqual(render_reference(enabled=(expected,), overrides={'Customer.Custom' + slot: invalid}), [])
                self.assertEqual(render_reference(enabled=(expected,), overrides={'Customer.CustomCaption' + slot: 'Unrelated field'}), [])

    def test_no_customer_and_quotes_hidden(self):
        self.assertEqual(render_reference(overrides={'Customer.ID': Decimal(0)}), [])
        self.assertEqual(render_reference(overrides={'Transaction.Type': Decimal(3)}), [])

    def test_sales_receipt_and_revised_file(self):
        rows = render_reference('178', '19.07', overrides={'Transaction.Type': Decimal(0)})
        self.assertEqual(rows, [['VAT-exclusive:', '|', Decimal('158.93')],
                               ['WTax 1%', '|', Decimal('1.59')],
                               ['WTax 2%', '|', Decimal('3.18')],
                               ['WTax 5%', '|', Decimal('7.95')],
                               ['WTax 15%', '|', Decimal('23.84')]])
        revised = ROOT / RELATIVE.replace('.xml', '_revised.xml')
        self.assertEqual(canonical(ET.parse(revised).getroot()), canonical(XML))

    def test_readable_font_and_currency_types(self):
        self.assertEqual(SECTION.find('.//TABLE/FONT').text.strip(), '"SmallBold"')
        for name in ('WTaxBase', 'WTaxAmount1', 'WTaxAmount2', 'WTaxAmount5', 'WTaxAmount15'):
            self.assertEqual(SECTION.find(f".//SET[@name='{name}']").attrib['type'], 'vbCurrency')
        self.assertAlmostEqual(sum(float(n.text.split('*')[1]) for n in SECTION.findall('.//TABLE/COLUMNHEADER/WIDTH')), .8)


if __name__ == '__main__':
    unittest.main(verbosity=2)
