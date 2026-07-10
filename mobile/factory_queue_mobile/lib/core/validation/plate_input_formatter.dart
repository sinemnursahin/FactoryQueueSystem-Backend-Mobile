import 'package:flutter/services.dart';

class PlateInputFormatter extends TextInputFormatter {
  const PlateInputFormatter();

  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    final formatted = newValue.text
        .toUpperCase()
        .replaceAll(RegExp(r'[^0-9A-ZÇĞİÖŞÜ\s]'), '')
        .replaceAll(RegExp(r'\s+'), ' ');

    return TextEditingValue(
      text: formatted,
      selection: TextSelection.collapsed(offset: formatted.length),
    );
  }
}
