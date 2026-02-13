// lib/services/payment_service.dart
import '../models/models.dart';
import 'api_service.dart';

class PaymentService {
  Future<PaymentModel> create(PaymentRequest req) async {
    final res = await apiService.dio.post('/Payment', data: req.toJson());
    return PaymentModel.fromJson((res.data as Map<String, dynamic>)['data'] as Map<String, dynamic>);
  }

  Future<List<PaymentModel>> getAll() async {
    final res = await apiService.dio.get('/Payment');
    final list = (res.data as Map<String, dynamic>)['data'] as List<dynamic>;
    return list.map((e) => PaymentModel.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<bool> delete(int id) async {
    final res = await apiService.dio.delete('/Payment/$id');
    return (res.data as Map<String, dynamic>)['success'] == true;
  }
}

final PaymentService paymentService = PaymentService();
