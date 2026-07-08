class LoginRequest {
  LoginRequest({required this.emailOrPhone, required this.password});

  final String emailOrPhone;
  final String password;

  Map<String, dynamic> toJson() => {
        'emailOrPhone': emailOrPhone,
        'password': password,
      };
}
