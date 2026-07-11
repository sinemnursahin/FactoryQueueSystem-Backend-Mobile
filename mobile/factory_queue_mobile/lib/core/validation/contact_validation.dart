class ContactValidation {
  static const invalidEmailMessage = 'Geçerli bir e-posta adresi giriniz.';
  static const invalidPhoneMessage = 'Geçerli bir Türkiye cep telefonu numarası giriniz.';
  static const invalidPlateMessage = 'Geçerli bir Türkiye plakası giriniz.';
  static const missingContactMessage = 'E-posta veya telefon numarasından en az biri girilmelidir.';

  static final _emailRegex = RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$');

  static String? emptyToNull(String value) {
    final trimmed = value.trim();
    return trimmed.isEmpty ? null : trimmed;
  }

  static String? normalizeEmail(String? value) {
    final trimmed = value?.trim();
    return trimmed == null || trimmed.isEmpty ? null : trimmed.toLowerCase();
  }

  static String? normalizePhone(String? value) {
    final trimmed = value?.trim();
    if (trimmed == null || trimmed.isEmpty) {
      return null;
    }

    var normalized = trimmed.replaceAll(RegExp(r'[\s()-]'), '');
    if (normalized.startsWith('+90')) {
      return normalized;
    } else if (normalized.startsWith('90')) {
      return '+$normalized';
    } else if (normalized.length == 11 && normalized.startsWith('05')) {
      return '+90${normalized.substring(1)}';
    } else if (normalized.length == 10 && normalized.startsWith('5')) {
      return '+90$normalized';
    }

    return normalized;
  }

  static bool isValidEmail(String email) => _emailRegex.hasMatch(email);

  static bool isValidPhone(String phone) => RegExp(r'^\+905\d{9}$').hasMatch(phone);

  static String phoneInputValue(String? value) {
    final normalized = normalizePhone(value);
    return normalized != null && isValidPhone(normalized) ? normalized.substring(3) : value?.trim() ?? '';
  }

  static String? normalizePlate(String? value) {
    final trimmed = value?.trim();
    if (trimmed == null || trimmed.isEmpty) {
      return null;
    }

    return trimmed.toUpperCase();
  }

  static bool isValidPlate(String? plate) {
    if (plate == null) {
      return false;
    }

    final match = RegExp(r'^(\d{2})[A-ZÇĞİÖŞÜ]{1,3}\d{2,5}$').firstMatch(plate);
    if (match == null) {
      return false;
    }

    final province = int.tryParse(match.group(1)!);
    return province != null && province >= 1 && province <= 81;
  }
}
