// lib/services/zakath_service.dart
import '../models/models.dart';
import 'api_service.dart';

class ZakathService {
  Future<ZakathResult> calculate(CalculateZakathRequest req) async {
    final res = await apiService.dio.post('/Zakath/calculate', data: req.toJson());
    return ZakathResult.fromJson((res.data as Map<String, dynamic>)['data'] as Map<String, dynamic>);
  }

  Future<List<ZakathResult>> getHistory() async {
    final res = await apiService.dio.get('/Zakath/history');
    final list = (res.data as Map<String, dynamic>)['data'] as List<dynamic>;
    return list.map((e) => ZakathResult.fromJson(e as Map<String, dynamic>)).toList();
  }
}

final ZakathService zakathService = ZakathService();
