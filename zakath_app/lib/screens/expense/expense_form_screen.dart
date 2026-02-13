// lib/screens/expense/expense_form_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../models/models.dart';
import '../../providers/auth_provider.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';

class ExpenseFormScreen extends ConsumerStatefulWidget {
  const ExpenseFormScreen({super.key});
  @override ConsumerState<ExpenseFormScreen> createState() => _ExpenseFormScreenState();
}

class _ExpenseFormScreenState extends ConsumerState<ExpenseFormScreen> {
  final _formKey     = GlobalKey<FormState>();
  final _amountCtrl  = TextEditingController();
  final _accountCtrl = TextEditingController();
  final _notesCtrl   = TextEditingController();
  DateTime _date     = DateTime.now();
  bool _loading      = false;
  String? _error;

  @override void dispose() { _amountCtrl.dispose(); _accountCtrl.dispose(); _notesCtrl.dispose(); super.dispose(); }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    final user = ref.read(authProvider);
    if (user == null) return;
    setState(() { _loading = true; _error = null; });
    try {
      await ref.read(expenseListProvider.notifier).add(ExpenseRequest(
        userId: user.userId, amount: double.parse(_amountCtrl.text.trim()),
        dateOfTransaction: _date,
        accountDeductedFrom: _accountCtrl.text.trim().isEmpty ? null : _accountCtrl.text.trim(),
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
    appBar: AppBar(backgroundColor: AppColors.expenseColor, foregroundColor: Colors.white, title: const Text('Add Expense')),
    body: SingleChildScrollView(padding: const EdgeInsets.all(20), child: Form(key: _formKey, child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        TextFormField(controller: _amountCtrl, decoration: _dec('Amount', Icons.attach_money),
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          validator: (v) { if (v == null || v.trim().isEmpty) return 'Enter amount'; if (double.tryParse(v.trim()) == null) return 'Invalid'; return null; }),
        const SizedBox(height: 14),
        InkWell(onTap: _pickDate, child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
          decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(12), border: Border.all(color: Colors.grey.shade300)),
          child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            Text('Date: ${_date.year}-${_date.month.toString().padLeft(2,'0')}-${_date.day.toString().padLeft(2,'0')}'),
            const Icon(Icons.calendar_today, color: AppColors.expenseColor),
          ]))),
        const SizedBox(height: 14),
        TextFormField(controller: _accountCtrl, decoration: _dec('Account / Purpose (optional)', Icons.wallet)),
        const SizedBox(height: 14),
        TextFormField(controller: _notesCtrl, decoration: _dec('Notes (optional)', Icons.note), maxLines: 3),
        const SizedBox(height: 8),
        if (_error != null) Padding(padding: const EdgeInsets.only(bottom: 8),
          child: Text(_error!, style: const TextStyle(color: AppColors.error), textAlign: TextAlign.center)),
        SizedBox(height: 48, child: ElevatedButton(
          onPressed: _loading ? null : _submit,
          style: ElevatedButton.styleFrom(backgroundColor: AppColors.expenseColor, foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
          child: _loading
              ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5))
              : const Text('Save Expense', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)))),
      ],
    ))),
  );

  InputDecoration _dec(String label, IconData icon) => InputDecoration(
    labelText: label, prefixIcon: Icon(icon, color: AppColors.expenseColor),
    filled: true, fillColor: Colors.white,
    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
    focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide(color: AppColors.expenseColor, width: 2)),
  );
}
