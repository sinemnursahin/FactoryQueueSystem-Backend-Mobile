class ShipmentResult {
  ShipmentResult({
    required this.shipmentId,
    required this.loadedWeight,
    required this.emptyWeight,
    required this.netAmount,
    required this.loadedWeighDate,
    required this.emptyWeighDate,
    required this.completedAt,
  });

  final String shipmentId;
  final double loadedWeight;
  final double emptyWeight;
  final double netAmount;
  final DateTime loadedWeighDate;
  final DateTime emptyWeighDate;
  final DateTime completedAt;

  factory ShipmentResult.fromJson(Map<String, dynamic> json) {
    return ShipmentResult(
      shipmentId: json['shipmentId'] as String,
      loadedWeight: (json['loadedWeight'] as num).toDouble(),
      emptyWeight: (json['emptyWeight'] as num).toDouble(),
      netAmount: (json['netAmount'] as num).toDouble(),
      loadedWeighDate: DateTime.parse(json['loadedWeighDate'] as String),
      emptyWeighDate: DateTime.parse(json['emptyWeighDate'] as String),
      completedAt: DateTime.parse(json['completedAt'] as String),
    );
  }
}
