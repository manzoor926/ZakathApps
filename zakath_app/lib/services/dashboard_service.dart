// lib/services/dashboard_service.dart
import '../models/models.dart';
import 'api_service.dart';

class DashboardService {
  Future<DashboardStats> getStats() async {
    final res = await apiService.dio.get('/Dashboard/stats');
    return DashboardStats.fromJson((res.data as Map<String, dynamic>)['data'] as Map<String, dynamic>);
  }

  Future<List<RecentTransaction>> getRecentTransactions() async {
    final res = await apiService.dio.get('/Dashboard/recent-transactions');
    final list = (res.data as Map<String, dynamic>)['data'] as List<dynamic>;
    return list.map((e) => RecentTransaction.fromJson(e as Map<String, dynamic>)).toList();
  }
}

final DashboardService dashboardService = DashboardService();
