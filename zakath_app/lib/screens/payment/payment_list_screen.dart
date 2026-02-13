// lib/screens/payment/payment_list_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';
import 'payment_form_screen.dart';

class PaymentListScreen extends ConsumerStatefulWidget {
  const PaymentListScreen({super.key});
  @override ConsumerState<PaymentListScreen> createState() => _PaymentListScreenState();
}

class _PaymentListScreenState extends ConsumerState<PaymentListScreen> {
  @override
  void initState() {
    super.initState();
    SchedulerBinding.instance.addPostFrameCallback((_) => ref.read(paymentListProvider.notifier).fetch());
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(paymentListProvider);
    return Stack(children: [
      state.when(
        data: (list) => list.isEmpty
            ? const Center(child: Text('No payments recorded yet.', style: TextStyle(color: AppColors.textSecondary)))
            : ListView.builder(padding: const EdgeInsets.all(12), itemCount: list.length, itemBuilder: (ctx, i) {
                final item = list[i];
                return Dismissible(
                  key: ValueKey(item.paymentId),
                  direction: DismissDirection.endToStart,
                  background: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.error, borderRadius: BorderRadius.circular(12)),
                    alignment: AlignmentDirectional.centerEnd, padding: const EdgeInsets.only(right: 20),
                    child: const Icon(Icons.delete, color: Colors.white)),
                  onDismissed: (_) => ref.read(paymentListProvider.notifier).remove(item.paymentId),
                  child: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(12),
                      boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 5, offset: const Offset(0, 2))]),
                    child: ListTile(
                      leading: CircleAvatar(backgroundColor: AppColors.accent.withOpacity(0.18),
                        child: const Icon(Icons.payment, color: AppColors.accent)),
                      title: Text('Payment: \$${item.paymentAmount.toStringAsFixed(2)}',
                        style: const TextStyle(fontWeight: FontWeight.w600)),
                      subtitle: Text(
                        '${item.recipientName ?? ""}  •  ${item.hijriDate ?? item.paymentDate.toString().substring(0, 10)}',
                        style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
                      trailing: item.isVerified
                          ? const Icon(Icons.verified, color: AppColors.success)
                          : const Icon(Icons.pending, color: AppColors.warning),
                    )),
                );
              }),
        loading: () => const Center(child: CircularProgressIndicator(color: AppColors.primary)),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
      Positioned(bottom: 20, right: 20, child: FloatingActionButton(
        backgroundColor: AppColors.accent, foregroundColor: Colors.white,
        onPressed: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const PaymentFormScreen())),
        child: const Icon(Icons.add, size: 28))),
    ]);
  }
}