class Profile {
  Profile({
    required this.id,
    required this.firstName,
    required this.lastName,
    this.email,
    this.phoneNumber,
  });

  final String id;
  final String firstName;
  final String lastName;
  final String? email;
  final String? phoneNumber;

  factory Profile.fromJson(Map<String, dynamic> json) {
    return Profile(
      id: json['id'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      email: json['email'] as String?,
      phoneNumber: json['phoneNumber'] as String?,
    );
  }
}
