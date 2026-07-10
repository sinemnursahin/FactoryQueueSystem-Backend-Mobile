class ProfileVehicle {
  ProfileVehicle({required this.id, required this.plateNumber});

  final String id;
  final String plateNumber;

  factory ProfileVehicle.fromJson(Map<String, dynamic> json) {
    return ProfileVehicle(
      id: json['id'] as String,
      plateNumber: json['plateNumber'] as String,
    );
  }
}
