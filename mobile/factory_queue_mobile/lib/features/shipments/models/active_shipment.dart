class ActiveShipment {
  ActiveShipment({
    required this.id,
    required this.status,
    required this.createdAt,
    required this.totalQueuedVehicles,
    this.queueNumber,
    this.vehicleId,
    this.plateNumber,
    this.driverName,
    this.statusName,
    this.queueDate,
    this.rawMaterialName,
    this.supplierName,
    this.queuedAt,
    this.vehiclesAhead,
  });

  final String id;
  final String? vehicleId;
  final String? plateNumber;
  final String? driverName;
  final int status;
  final String? statusName;
  final int? queueNumber;
  final String? queueDate;
  final String? rawMaterialName;
  final String? supplierName;
  final DateTime createdAt;
  final DateTime? queuedAt;
  final int totalQueuedVehicles;
  final int? vehiclesAhead;

  bool get canQueue => status == 0;
  bool get hasVehicle => plateNumber != null && plateNumber!.isNotEmpty;

  String get displayStatusName => switch (status) {
        0 => 'Yolda',
        1 => 'Sırada',
        2 => 'Kantara Çağrıldı',
        3 => 'Kantarda',
        4 => 'Boşaltımda',
        5 => 'Boşaltım Tamamlandı',
        6 => 'Tamamlandı',
        _ => statusName ?? '-',
      };

  factory ActiveShipment.fromJson(Map<String, dynamic> json) {
    return ActiveShipment(
      id: json['id'] as String,
      vehicleId: json['vehicleId'] as String?,
      plateNumber: json['plateNumber'] as String?,
      driverName: json['driverName'] as String?,
      status: json['status'] as int,
      statusName: json['statusName'] as String?,
      totalQueuedVehicles: json['totalQueuedVehicles'] as int? ?? 0,
      queueNumber: json['queueNumber'] as int?,
      queueDate: json['queueDate'] as String?,
      rawMaterialName: json['rawMaterialName'] as String?,
      supplierName: json['supplierName'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
      queuedAt: json['queuedAt'] == null ? null : DateTime.parse(json['queuedAt'] as String),
      vehiclesAhead: json['vehiclesAhead'] as int?,
    );
  }
}
