import 'package:flutter/services.dart';

class PhoneInputFormatter extends TextInputFormatter {
  const PhoneInputFormatter();

  static String digitsOnly(String value) => value.replaceAll(RegExp(r'\D'), '');

  static String format(String value) {
    final digits = digitsOnly(value);
    final limited = digits.length > 10 ? digits.substring(0, 10) : digits;
    final buffer = StringBuffer();

    for (var i = 0; i < limited.length; i++) {
      if (i == 3 || i == 6 || i == 8) {
        buffer.write(' ');
      }
      buffer.write(limited[i]);
    }

    return buffer.toString();
  }

  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    final formatted = format(newValue.text);
    return TextEditingValue(
      text: formatted,
      selection: TextSelection.collapsed(offset: formatted.length),
    );
  }
}
