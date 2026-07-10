import 'dart:ui';

import 'package:factory_queue_mobile/core/validation/contact_validation.dart';
import 'package:factory_queue_mobile/features/auth/providers/auth_provider.dart';
import 'package:factory_queue_mobile/features/auth/screens/register_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _emailOrPhoneController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _emailOrPhoneController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(authProvider, (previous, next) {
      if (previous?.isLoading == true && next.error != null) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(next.error!), backgroundColor: const Color(0xFFEF4444)));
      }
      if (previous?.isLoading == true && next.isAuthenticated) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Giriş başarılı.')));
      }
    });

    final auth = ref.watch(authProvider);

    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [Color(0xFFE0F2FE), Color(0xFFF5F7FB), Color(0xFFDCFCE7)],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 460),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(24),
                  child: BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 260),
                      padding: const EdgeInsets.all(24),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: .78),
                        borderRadius: BorderRadius.circular(24),
                        border: Border.all(color: Colors.white.withValues(alpha: .7)),
                        boxShadow: [
                          BoxShadow(
                            color: const Color(0xFF0EA5E9).withValues(alpha: .10),
                            blurRadius: 34,
                            offset: const Offset(0, 20),
                          ),
                        ],
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          const Hero(
                            tag: 'app-logo',
                            child: CircleAvatar(
                              radius: 34,
                              backgroundColor: Color(0xFF0EA5E9),
                              child: Icon(Icons.factory_rounded, color: Colors.white, size: 34),
                            ),
                          ),
                          const SizedBox(height: 24),
                          Text('Hoş geldiniz', textAlign: TextAlign.center, style: Theme.of(context).textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.w800)),
                          const SizedBox(height: 8),
                          Text('Hammadde kabul sıranızı takip edin.', textAlign: TextAlign.center, style: Theme.of(context).textTheme.bodyLarge?.copyWith(color: Colors.black54)),
                          const SizedBox(height: 28),
                          TextField(
                            controller: _emailOrPhoneController,
                            keyboardType: TextInputType.emailAddress,
                            decoration: const InputDecoration(prefixIcon: Icon(Icons.alternate_email_rounded), labelText: 'E-posta veya telefon', helperText: 'Örn: ad@example.com'),
                          ),
                          const SizedBox(height: 14),
                          TextField(
                            controller: _passwordController,
                            obscureText: _obscurePassword,
                            decoration: InputDecoration(
                              prefixIcon: const Icon(Icons.lock_rounded),
                              labelText: 'Şifre',
                              suffixIcon: IconButton(
                                onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                                icon: Icon(_obscurePassword ? Icons.visibility_rounded : Icons.visibility_off_rounded),
                              ),
                            ),
                          ),
                          const SizedBox(height: 24),
                          FilledButton(
                            onPressed: auth.isLoading
                                ? null
                                : () {
                                    final login = _emailOrPhoneController.text.trim();
                                    if (login.contains('@') && !ContactValidation.isValidEmail(login.toLowerCase())) {
                                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text(ContactValidation.invalidEmailMessage), backgroundColor: Color(0xFFEF4444)));
                                      return;
                                    }

                                    ref.read(authProvider.notifier).login(
                                          login,
                                          _passwordController.text,
                                        );
                                  },
                            child: AnimatedSwitcher(
                              duration: const Duration(milliseconds: 180),
                              child: auth.isLoading
                                  ? const SizedBox(key: ValueKey('loading'), height: 22, width: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                                  : const Text(key: ValueKey('text'), 'Giriş Yap'),
                            ),
                          ),
                          const SizedBox(height: 12),
                          TextButton(
                            onPressed: auth.isLoading ? null : () => Navigator.of(context).push(_fadeRoute(const RegisterScreen())),
                            child: const Text('Yeni şoför hesabı oluştur'),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

PageRouteBuilder<void> _fadeRoute(Widget page) {
  return PageRouteBuilder<void>(
    pageBuilder: (_, animation, _) => FadeTransition(opacity: animation, child: page),
  );
}
