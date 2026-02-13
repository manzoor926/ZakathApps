// lib/screens/auth/register_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../models/models.dart';
import '../../providers/auth_provider.dart';
import '../../utils/constants.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});
  @override ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _formKey     = GlobalKey<FormState>();
  final _nameCtrl    = TextEditingController();
  final _mobileCtrl  = TextEditingController();
  final _emailCtrl   = TextEditingController();
  final _passCtrl    = TextEditingController();
  final _confirmCtrl = TextEditingController();
  bool _loading = false;
  String? _error;

  @override void dispose() {
    _nameCtrl.dispose(); _mobileCtrl.dispose(); _emailCtrl.dispose();
    _passCtrl.dispose(); _confirmCtrl.dispose(); super.dispose();
  }

  Future<void> _register() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    try {
      final result = await ref.read(authProvider.notifier).register(RegisterRequest(
        fullName: _nameCtrl.text.trim(), mobileNumber: _mobileCtrl.text.trim(),
        email: _emailCtrl.text.trim().isEmpty ? null : _emailCtrl.text.trim(),
        password: _passCtrl.text.trim(), confirmPassword: _confirmCtrl.text.trim(),
      ));
      if (result['success'] == true) {
        if (mounted) Navigator.pushReplacementNamed(context, '/login');
      } else {
        setState(() { _error = result['message'] ?? 'Registration failed'; });
      }
    } catch (e) { setState(() { _error = e.toString(); }); }
    finally { if (mounted) setState(() { _loading = false; }); }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    backgroundColor: AppColors.background,
    appBar: AppBar(backgroundColor: AppColors.primary, foregroundColor: Colors.white, title: const Text('Register')),
    body: SingleChildScrollView(padding: const EdgeInsets.all(24), child: Form(key: _formKey, child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const SizedBox(height: 10),
        TextFormField(controller: _nameCtrl, decoration: _dec('Full Name', Icons.person),
          validator: (v) => (v == null || v.trim().isEmpty) ? 'Enter your name' : null),
        const SizedBox(height: 14),
        TextFormField(controller: _mobileCtrl, decoration: _dec('Mobile Number', Icons.phone_android),
          keyboardType: TextInputType.phone,
          validator: (v) => (v == null || v.trim().isEmpty) ? 'Enter mobile number' : null),
        const SizedBox(height: 14),
        TextFormField(controller: _emailCtrl, decoration: _dec('Email (optional)', Icons.email),
          keyboardType: TextInputType.emailAddress),
        const SizedBox(height: 14),
        TextFormField(controller: _passCtrl, decoration: _dec('Password', Icons.lock), obscureText: true,
          validator: (v) => (v == null || v.trim().length < 8) ? 'Min 8 characters' : null),
        const SizedBox(height: 14),
        TextFormField(controller: _confirmCtrl, decoration: _dec('Confirm Password', Icons.lock_outline), obscureText: true,
          validator: (v) => v != _passCtrl.text ? 'Passwords do not match' : null),
        const SizedBox(height: 8),

        if (_error != null) Padding(padding: const EdgeInsets.only(bottom: 8),
          child: Text(_error!, style: const TextStyle(color: AppColors.error), textAlign: TextAlign.center)),

        SizedBox(height: 48, child: ElevatedButton(
          onPressed: _loading ? null : _register,
          style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
          child: _loading
              ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5))
              : const Text('Register', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
        )),
        const SizedBox(height: 18),
        Row(mainAxisAlignment: MainAxisAlignment.center, children: [
          const Text('Already have an account? '),
          GestureDetector(onTap: () => Navigator.pushReplacementNamed(context, '/login'),
            child: const Text('Login', style: TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold))),
        ]),
      ],
    ))),
  );

  InputDecoration _dec(String label, IconData icon) => InputDecoration(
    labelText: label, prefixIcon: Icon(icon, color: AppColors.primary),
    filled: true, fillColor: Colors.white,
    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
    focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12),
      borderSide: BorderSide(color: AppColors.primary, width: 2)),
  );
}
