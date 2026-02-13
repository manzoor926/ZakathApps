// lib/screens/zakath/zakath_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../models/models.dart';
import '../../providers/auth_provider.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';
import '../payment/payment_form_screen.dart';

class ZakathScreen extends ConsumerStatefulWidget {
  const ZakathScreen({super.key});
  @override ConsumerState<ZakathScreen> createState() => _ZakathScreenState();
}

class _ZakathScreenState extends ConsumerState<ZakathScreen> {
  ZakathResult? _latest;
  bool _calculating = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    SchedulerBinding.instance.addPostFrameCallback((_) => ref.read(zakathHistoryProvider.notifier).fetchHistory());
  }

  Future<void> _calculate() async {
    final user = ref.read(authProvider);
    if (user == null) return;
    setState(() { _calculating = true; _error = null; });
    try {
      _latest = await ref.read(zakathHistoryProvider.notifier).calculate(
        CalculateZakathRequest(userId: user.userId));
      setState(() {});
    } catch (e) { setState(() { _error = e.toString(); }); }
    finally { if (mounted) setState(() { _calculating = false; }); }
  }

  @override
  Widget build(BuildContext context) {
    final history = ref.watch(zakathHistoryProvider);
    return SingleChildScrollView(padding: const EdgeInsets.all(16), child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        // Calculate button
        SizedBox(height: 50, child: ElevatedButton.icon(
          icon: _calculating
              ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5))
              : const Icon(Icons.calculate),
          label: const Text('Calculate Zakath', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
          onPressed: _calculating ? null : _calculate,
          style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white,
            shape:  RoundedRectangleBorder(borderRadius: BorderRadius.circular(14))))),

        if (_error != null) Padding(padding: const EdgeInsets.only(top: 10),
          child: Text(_error!, style: const TextStyle(color: AppColors.error), textAlign: TextAlign.center)),

        // Latest result
        if (_latest != null) ...[const SizedBox(height: 18), _resultCard(_latest!)],

        const SizedBox(height: 22),
        const Text('History', style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
        const SizedBox(height: 8),

        history.when(
          data: (list) => list.isEmpty
              ? const Text('No calculations yet.', style: TextStyle(color: AppColors.textSecondary))
              : Column(children: list.map(_historyRow).toList()),
          loading: () => const Center(child: CircularProgressIndicator(color: AppColors.primary)),
          error: (e, _) => Text('Error: $e'),
        ),
      ],
    ));
  }

  Widget _resultCard(ZakathResult r) {
    final c = r.isZakathDue ? AppColors.warning : AppColors.success;
    return Container(
      decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(16),
        border: Border.all(color: c, width: 1.5),
        boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 8, offset: Offset(0, 3))]),
      padding: const EdgeInsets.all(18),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
          Text(r.isZakathDue ? '⚠️  Zakath is Due' : '✅  Up to Date',
            style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: c)),
          Text(r.hijriDate ?? '', style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
        ]),
        const SizedBox(height: 14),
        _row('Net Worth',         r.netWorth),
        _row('Total Assets',      r.totalAssets),
        _row('Total Liabilities', r.totalLiabilities),
        _row('Nisab Threshold',   r.nisabThreshold),
        const Divider(height: 18),
        _row('Zakath Amount',     r.zakathAmount, bold: true, color: c),

        if (r.breakdown != null) ...[
          const SizedBox(height: 14),
          const Text('Breakdown', style: TextStyle(fontWeight: FontWeight.bold, color: AppColors.textSecondary, fontSize: 13)),
          const SizedBox(height: 6),
          _bRow('Cash',        r.breakdown!.cash,        Icons.money),
          _bRow('Gold',        r.breakdown!.gold,        Icons.star),
          _bRow('Silver',      r.breakdown!.silver,      Icons.brightness_5),
          _bRow('Investments', r.breakdown!.investments, Icons.trending_up),
          _bRow('Other',       r.breakdown!.otherAssets, Icons.folder_open),
        ],

        if (r.isZakathDue) ...[
          const SizedBox(height: 16),
          SizedBox(height: 44, child: ElevatedButton.icon(
            icon: const Icon(Icons.payment),
            label: const Text('Pay Zakath Now'),
            onPressed: () => Navigator.push(context,
              MaterialPageRoute(builder: (_) => PaymentFormScreen(calculationId: r.calculationId, defaultAmount: r.zakathAmount))),
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.accent, foregroundColor: Colors.white,
              shape:  RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))))),
        ],
      ]),
    );
  }

  Widget _row(String label, double val, {bool bold = false, Color? color}) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 3),
    child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
      Text(label, style: const TextStyle(color: AppColors.textSecondary)),
      Text('\$${val.toStringAsFixed(2)}', style: TextStyle(color: color ?? AppColors.textPrimary,
        fontWeight: bold ? FontWeight.bold : FontWeight.normal, fontSize: bold ? 18 : 14)),
    ]),
  );

  Widget _bRow(String label, double val, IconData icon) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 2),
    child: Row(children: [
      Icon(icon, size: 18, color: AppColors.primary), const SizedBox(width: 8),
      Expanded(child: Text(label, style: const TextStyle(fontSize: 13))),
      Text('\$${val.toStringAsFixed(2)}', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
    ]),
  );

  Widget _historyRow(ZakathResult r) {
    final c = r.isZakathDue ? AppColors.warning : AppColors.success;
    return Container(margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(12),
        boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 4, offset: const Offset(0, 1))]),
      child: ListTile(
        leading: CircleAvatar(backgroundColor: c.withOpacity(0.15),
          child: Icon(r.isZakathDue ? Icons.warning_amber : Icons.check_circle, color: c)),
        title: Text('Zakath: \$${r.zakathAmount.toStringAsFixed(2)}', style: const TextStyle(fontWeight: FontWeight.w600)),
        subtitle: Text(r.hijriDate ?? r.calculationDate.toString().substring(0, 10),
          style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
        trailing: Text(r.isZakathDue ? 'Due' : 'OK', style: TextStyle(color: c, fontWeight: FontWeight.bold)),
      ));
  }
}
