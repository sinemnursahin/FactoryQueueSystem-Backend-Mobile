class AuthResponse {
  AuthResponse({
    required this.token,
    required this.userId,
    required this.firstName,
    required this.lastName,
    required this.role,
    this.email,
    this.phoneNumber,
  });

  final String token;
  final String userId;
  final String firstName;
  final String lastName;
  final String? email;
  final String? phoneNumber;
  final String role;

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      token: json['token'] as String,
      userId: json['userId'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      email: json['email'] as String?,
      phoneNumber: json['phoneNumber'] as String?,
      role: json['role'] as String,
    );
  }
}
