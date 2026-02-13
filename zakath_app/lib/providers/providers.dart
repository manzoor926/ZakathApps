// lib/providers/providers.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/models.dart';
import '../services/income_service.dart';
import '../services/expense_service.dart';
import '../services/asset_service.dart';
import '../services/zakath_service.dart';
import '../services/payment_service.dart';
import '../services/dashboard_service.dart';

// ─── INCOME ───
final incomeListProvider = StateNotifierProvider<IncomeNotifier, AsyncValue<List<IncomeModel>>>(
  (ref) => IncomeNotifier(),
);
class IncomeNotifier extends StateNotifier<AsyncValue<List<IncomeModel>>> {
  IncomeNotifier() : super(const AsyncValue.loading());
  Future<void> fetch() async {
    state = const AsyncValue.loading();
    try { state = AsyncValue.data(await incomeService.getAll()); }
    catch (e, s) { state = AsyncValue.error(e, s); }
  }
  Future<void> add(IncomeRequest req) async { await incomeService.create(req); await fetch(); }
  Future<void> remove(int id) async { await incomeService.delete(id); await fetch(); }
}

// ─── EXPENSE ───
final expenseListProvider = StateNotifierProvider<ExpenseNotifier, AsyncValue<List<ExpenseModel>>>(
  (ref) => ExpenseNotifier(),
);
class ExpenseNotifier extends StateNotifier<AsyncValue<List<ExpenseModel>>> {
  ExpenseNotifier() : super(const AsyncValue.loading());
  Future<void> fetch() async {
    state = const AsyncValue.loading();
    try { state = AsyncValue.data(await expenseService.getAll()); }
    catch (e, s) { state = AsyncValue.error(e, s); }
  }
  Future<void> add(ExpenseRequest req) async { await expenseService.create(req); await fetch(); }
  Future<void> remove(int id) async { await expenseService.delete(id); await fetch(); }
}

// ─── ASSET ───
final assetListProvider = StateNotifierProvider<AssetNotifier, AsyncValue<List<AssetModel>>>(
  (ref) => AssetNotifier(),
);
class AssetNotifier extends StateNotifier<AsyncValue<List<AssetModel>>> {
  AssetNotifier() : super(const AsyncValue.loading());
  Future<void> fetch() async {
    state = const AsyncValue.loading();
    try { state = AsyncValue.data(await assetService.getAll()); }
    catch (e, s) { state = AsyncValue.error(e, s); }
  }
  Future<void> add(AssetRequest req) async { await assetService.create(req); await fetch(); }
  Future<void> remove(int id) async { await assetService.delete(id); await fetch(); }
}

// ─── ZAKATH ───
final zakathHistoryProvider = StateNotifierProvider<ZakathNotifier, AsyncValue<List<ZakathResult>>>(
  (ref) => ZakathNotifier(),
);
class ZakathNotifier extends StateNotifier<AsyncValue<List<ZakathResult>>> {
  ZakathNotifier() : super(const AsyncValue.loading());
  Future<void> fetchHistory() async {
    state = const AsyncValue.loading();
    try { state = AsyncValue.data(await zakathService.getHistory()); }
    catch (e, s) { state = AsyncValue.error(e, s); }
  }
  Future<ZakathResult> calculate(CalculateZakathRequest req) async {
    final result = await zakathService.calculate(req);
    await fetchHistory();
    return result;
  }
}

// ─── PAYMENT ───
final paymentListProvider = StateNotifierProvider<PaymentNotifier, AsyncValue<List<PaymentModel>>>(
  (ref) => PaymentNotifier(),
);
class PaymentNotifier extends StateNotifier<AsyncValue<List<PaymentModel>>> {
  PaymentNotifier() : super(const AsyncValue.loading());
  Future<void> fetch() async {
    state = const AsyncValue.loading();
    try { state = AsyncValue.data(await paymentService.getAll()); }
    catch (e, s) { state = AsyncValue.error(e, s); }
  }
  Future<void> add(PaymentRequest req) async { await paymentService.create(req); await fetch(); }
  Future<void> remove(int id) async { await paymentService.delete(id); await fetch(); }
}

// ─── DASHBOARD ───
final dashboardStatsProvider = FutureProvider<DashboardStats>((ref) async {
  return await dashboardService.getStats();
});
final recentTransactionsProvider = FutureProvider<List<RecentTransaction>>((ref) async {
  return await dashboardService.getRecentTransactions();
});
