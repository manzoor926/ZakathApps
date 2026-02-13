// lib/screens/payment/payment_form_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../models/models.dart';
import '../../providers/auth_provider.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';

class PaymentFormScreen extends ConsumerStatefulWidget {
  final int? calculationId;
  final double? defaultAmount;
  const PaymentFormScreen({super.key, this.calculationId, this.defaultAmount});
  @override ConsumerState<PaymentFormScreen> createState() => _PaymentFormScreenState();
}

class _PaymentFormScreenState extends ConsumerState<PaymentFormScreen> {
  final _formKey        = GlobalKey<FormState>();
  late final _amountCtrl;
  final _recipientCtrl  = TextEditingController();
  final _categoryCtrl   = TextEditingController();
  final _methodCtrl     = TextEditingController();
  final _notesCtrl      = TextEditingController();
  DateTime _date        = DateTime.now();
  bool _loading         = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _amountCtrl = TextEditingController(
      text: widget.defaultAmount != null ? widget.defaultAmount!.toStringAsFixed(2) : '',
    );
  }

  @override void dispose() {
    _amountCtrl.dispose(); _recipientCtrl.dispose(); _categoryCtrl.dispose();
    _methodCtrl.dispose(); _notesCtrl.dispose(); super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    final user = ref.read(authProvider);
    if (user == null) return;
    setState(() { _loading = true; _error = null; });
    try {
      await ref.read(paymentListProvider.notifier).add(PaymentRequest(
        userId: user.userId,
        calculationId: widget.calculationId,
        paymentAmount: double.parse(_amountCtrl.text.trim()),
        paymentDate: _date,
        recipientName: _recipientCtrl.text.trim().isEmpty ? null : _recipientCtrl.text.trim(),
        recipientCategory: _categoryCtrl.text.trim().isEmpty ? null : _categoryCtrl.text.trim(),
        paymentMethod: _methodCtrl.text.trim().isEmpty ? null : _methodCtrl.text.trim(),
        notes: _notesCtrl.text.trim().isEmpty ? null : _notesCtrl.text.trim(),
      ));
      if (mounted) Navigator.pop(context);
    } catch (e) { setState(() { _error = e.toString(); }); }
    finally { if (mounted) setState(() { _loading = false; }); }
  }

  Future<void> _pickDate() async {
    final p = await showDatePicker(context: context, initialDate: _date, firstDate: DateTime(2020), lastDate: DateTime.now());
    if (p != null) setState(() { _date = p; });
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    backgroundColor: AppColors.background,
    appBar: AppBar(backgroundColor: AppColors.accent, foregroundColor: Colors.white, title: const Text('Pay Zakath')),
    body: SingleChildScrollView(padding: const EdgeInsets.all(20), child: Form(key: _formKey, child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        TextFormField(controller: _amountCtrl, decoration: _dec('Payment Amount', Icons.attach_money),
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          validator: (v) { if (v == null || v.trim().isEmpty) return 'Enter amount'; if (double.tryParse(v.trim()) == null) return 'Invalid'; return null; }),
        const SizedBox(height: 14),

        InkWell(onTap: _pickDate, child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
          decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(12), border: Border.all(color: Colors.grey.shade300)),
          child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            Text('Date: ${_date.year}-${_date.month.toString().padLeft(2,'0')}-${_date.day.toString().padLeft(2,'0')}'),
            const Icon(Icons.calendar_today, color: AppColors.accent),
          ]))),
        const SizedBox(height: 14),

        TextFormField(controller: _recipientCtrl, decoration: _dec('Recipient Name (optional)', Icons.person)),
        const SizedBox(height: 14),
        TextFormField(controller: _categoryCtrl, decoration: _dec('Recipient Category (optional)', Icons.category)),
        const SizedBox(height: 14),
        TextFormField(controller: _methodCtrl, decoration: _dec('Payment Method (optional)', Icons.payment)),
        const SizedBox(height: 14),
        TextFormField(controller: _notesCtrl, decoration: _dec('Notes (optional)', Icons.note), maxLines: 2),
        const SizedBox(height: 8),

        if (_error != null) Padding(padding: const EdgeInsets.only(bottom: 8),
          child: Text(_error!, style: const TextStyle(color: AppColors.error), textAlign: TextAlign.center)),

        SizedBox(height: 48, child: ElevatedButton(
          onPressed: _loading ? null : _submit,
          style: ElevatedButton.styleFrom(backgroundColor: AppColors.accent, foregroundColor: Colors.white,
            shape:  RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
          child: _loading
              ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5))
              : const Text('Record Payment', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)))),
      ],
    ))),
  );

  InputDecoration _dec(String label, IconData icon) => InputDecoration(
    labelText: label, prefixIcon: Icon(icon, color: AppColors.accent),
    filled: true, fillColor: Colors.white,
    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
    focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.accent, width: 2)),
  );
}
