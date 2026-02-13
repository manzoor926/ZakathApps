// lib/screens/auth/login_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../models/models.dart';
import '../../providers/auth_provider.dart';
import '../../utils/constants.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});
  @override ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey      = GlobalKey<FormState>();
  final _mobileCtrl   = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _loading = false;
  String? _error;

  @override void dispose() { _mobileCtrl.dispose(); _passwordCtrl.dispose(); super.dispose(); }

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    try {
      final r = await ref.read(authProvider.notifier).login(
        LoginRequest(mobileNumber: _mobileCtrl.text.trim(), password: _passwordCtrl.text.trim()),
      );
      if (!r.success) setState(() { _error = r.message; });
      // success → authProvider state flips → router rebuilds to AppShell
    } catch (e) {
      setState(() { _error = e.toString(); });
    } finally { if (mounted) setState(() { _loading = false; }); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Center(child: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: 28),
        child: Form(key: _formKey, child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Icon(Icons.mosque, size: 72, color: AppColors.primary),
            const SizedBox(height: 6),
            Text('Zakath App', textAlign: TextAlign.center,
              style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold, color: AppColors.primary)),
            const SizedBox(height: 32),

            TextFormField(
              controller: _mobileCtrl,
              decoration: _dec('Mobile Number', Icons.phone_android),
              keyboardType: TextInputType.phone,
              validator: (v) => (v == null || v.trim().isEmpty) ? 'Enter mobile number' : null,
            ),
            const SizedBox(height: 14),
            TextFormField(
              controller: _passwordCtrl,
              decoration: _dec('Password', Icons.lock),
              obscureText: true,
              validator: (v) => (v == null || v.trim().isEmpty) ? 'Enter password' : null,
            ),
            const SizedBox(height: 6),

            if (_error != null) Padding(padding: const EdgeInsets.only(bottom: 6),
              child: Text(_error!, style: const TextStyle(color: AppColors.error), textAlign: TextAlign.center)),

            SizedBox(height: 48, child: ElevatedButton(
              onPressed: _loading ? null : _login,
              style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
              child: _loading
                  ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5))
                  : const Text('Login', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
            )),
            const SizedBox(height: 18),

            Row(mainAxisAlignment: MainAxisAlignment.center, children: [
              const Text("Don't have an account? "),
              GestureDetector(
                onTap: () => Navigator.pushNamed(context, '/register'),
                child: const Text('Register', style: TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold)),
              ),
            ]),
          ],
        )),
      )),
    );
  }

  InputDecoration _dec(String label, IconData icon) => InputDecoration(
    labelText: label, prefixIcon: Icon(icon, color: AppColors.primary),
    filled: true, fillColor: Colors.white,
    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
    focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12),
      borderSide: BorderSide(color: AppColors.primary, width: 2)),
  );
}
