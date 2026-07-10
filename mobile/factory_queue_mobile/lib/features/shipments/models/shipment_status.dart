class ShipmentStatus {
  ShipmentStatus({
    required this.id,
    required this.status,
    required this.totalQueuedVehicles,
    this.queueNumber,
    this.queueDate,
    this.queuedAt,
    this.completedAt,
    this.statusName,
    this.vehiclesAhead,
  });

  final String id;
  final int status;
  final String? statusName;
  final int? queueNumber;
  final String? queueDate;
  final DateTime? queuedAt;
  final DateTime? completedAt;
  final int totalQueuedVehicles;
  final int? vehiclesAhead;

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

  factory ShipmentStatus.fromJson(Map<String, dynamic> json) {
    return ShipmentStatus(
      id: json['id'] as String,
      status: json['status'] as int,
      statusName: json['statusName'] as String?,
      totalQueuedVehicles: json['totalQueuedVehicles'] as int? ?? 0,
      queueNumber: json['queueNumber'] as int?,
      queueDate: json['queueDate'] as String?,
      queuedAt: json['queuedAt'] == null ? null : DateTime.parse(json['queuedAt'] as String),
      completedAt: json['completedAt'] == null ? null : DateTime.parse(json['completedAt'] as String),
      vehiclesAhead: json['vehiclesAhead'] as int?,
    );
  }
}
