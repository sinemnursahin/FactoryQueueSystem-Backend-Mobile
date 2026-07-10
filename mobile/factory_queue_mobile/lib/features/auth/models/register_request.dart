class RegisterRequest {
  RegisterRequest({
    required this.firstName,
    required this.lastName,
    required this.password,
    this.email,
    this.phoneNumber,
  });

  final String firstName;
  final String lastName;
  final String? email;
  final String? phoneNumber;
  final String password;

  Map<String, dynamic> toJson() => {
        'firstName': firstName,
        'lastName': lastName,
        'email': email,
        'phoneNumber': phoneNumber,
        'password': password,
      };
}
