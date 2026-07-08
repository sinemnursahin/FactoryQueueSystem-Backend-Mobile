import 'package:factory_queue_mobile/features/auth/providers/auth_provider.dart';
import 'package:factory_queue_mobile/features/auth/screens/login_screen.dart';
import 'package:factory_queue_mobile/features/shipments/providers/shipment_provider.dart';
import 'package:factory_queue_mobile/features/shipments/screens/active_shipment_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  runApp(const ProviderScope(child: FactoryQueueApp()));
}

class FactoryQueueApp extends ConsumerWidget {
  const FactoryQueueApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auth = ref.watch(authProvider);
    ref.listen<AuthState>(authProvider, (previous, next) {
      final previousUserId = previous?.user?.userId;
      final nextUserId = next.user?.userId;
      if (previousUserId != nextUserId) {
        ref.invalidate(activeShipmentProvider);
      }
    });

    return MaterialApp(
      title: 'Fabrika Sıra Sistemi',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.indigo),
        useMaterial3: true,
        scaffoldBackgroundColor: const Color(0xFFF6F7F9),
        appBarTheme: const AppBarTheme(centerTitle: false, elevation: 0),
        cardTheme: CardThemeData(
          elevation: 0,
          color: Colors.white,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          margin: EdgeInsets.zero,
        ),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(
            minimumSize: const Size.fromHeight(48),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
          filled: true,
          fillColor: Colors.white,
        ),
      ),
      home: KeyedSubtree(
        key: ValueKey(auth.user?.userId ?? 'guest'),
        child: auth.isAuthenticated ? const ActiveShipmentScreen() : const LoginScreen(),
      ),
    );
  }
}
