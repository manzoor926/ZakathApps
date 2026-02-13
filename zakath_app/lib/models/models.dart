// lib/models/models.dart
// Every class here maps 1:1 to a C# DTO.  JSON keys match exactly.

// ══════════════════════════════════════
// AUTH
// ══════════════════════════════════════
class RegisterRequest {
  final String fullName, mobileNumber, password, confirmPassword;
  final String? email;
  RegisterRequest({
    required this.fullName, required this.mobileNumber,
    this.email, required this.password, required this.confirmPassword,
  });
  Map<String, dynamic> toJson() => {
    'fullName': fullName, 'mobileNumber': mobileNumber,
    if (email != null) 'email': email,
    'password': password, 'confirmPassword': confirmPassword,
  };
}

class LoginRequest {
  final String mobileNumber, password;
  LoginRequest({required this.mobileNumber, required this.password});
  Map<String, dynamic> toJson() => {'mobileNumber': mobileNumber, 'password': password};
}

class UserModel {
  final int userId;
  final String fullName, mobileNumber;
  final String? email, preferredLanguage;
  final int? preferredMadhabId;
  final bool notificationEnabled;

  UserModel({
    required this.userId, required this.fullName, required this.mobileNumber,
    this.email, this.preferredLanguage, this.preferredMadhabId,
    this.notificationEnabled = true,
  });

  factory UserModel.fromJson(Map<String, dynamic> j) => UserModel(
    userId: j['userID'] ?? 0, fullName: j['fullName'] ?? '',
    mobileNumber: j['mobileNumber'] ?? '', email: j['email'],
    preferredLanguage: j['preferredLanguage'],
    preferredMadhabId: j['preferredMadhabID'],
    notificationEnabled: j['notificationEnabled'] ?? true,
  );
}

class LoginResponse {
  final bool success;
  final String message;
  final String? token;
  final UserModel? user;
  LoginResponse({required this.success, required this.message, this.token, this.user});
  factory LoginResponse.fromJson(Map<String, dynamic> j) => LoginResponse(
    success: j['success'] ?? false, message: j['message'] ?? '',
    token: j['token'],
    user: j['user'] != null ? UserModel.fromJson(j['user']) : null,
  );
}

// ══════════════════════════════════════
// INCOME
// ══════════════════════════════════════
class IncomeRequest {
  final int userId;
  final double amount;
  final DateTime dateReceived;
  final String? sourceName, notes;
  final int? categoryId, currencyId;
  final bool isRecurring, isZakathEligible;

  IncomeRequest({
    required this.userId, required this.amount, required this.dateReceived,
    this.sourceName, this.notes, this.categoryId, this.currencyId,
    this.isRecurring = false, this.isZakathEligible = true,
  });
  Map<String, dynamic> toJson() => {
    'userID': userId, 'amount': amount,
    'dateReceived': dateReceived.toIso8601String(),
    if (categoryId != null) 'categoryID': categoryId,
    if (sourceName != null) 'sourceName': sourceName,
    if (notes != null) 'notes': notes,
    'isRecurring': isRecurring, 'isZakathEligible': isZakathEligible,
    if (currencyId != null) 'currencyID': currencyId,
  };
}

class IncomeModel {
  final int incomeId;
  final double amount;
  final DateTime dateReceived;
  final String? hijriDate, categoryName, sourceName, notes, currencySymbol;
  final bool isRecurring, isZakathEligible;

  IncomeModel({
    required this.incomeId, required this.amount, required this.dateReceived,
    this.hijriDate, this.categoryName, this.sourceName, this.notes,
    this.currencySymbol, this.isRecurring = false, this.isZakathEligible = true,
  });
  factory IncomeModel.fromJson(Map<String, dynamic> j) => IncomeModel(
    incomeId: j['incomeID'] ?? 0, amount: (j['amount'] as num).toDouble(),
    dateReceived: DateTime.parse(j['dateReceived']),
    hijriDate: j['hijriDateReceived'], categoryName: j['categoryName'],
    sourceName: j['sourceName'], notes: j['notes'],
    currencySymbol: j['currencySymbol'],
    isRecurring: j['isRecurring'] ?? false, isZakathEligible: j['isZakathEligible'] ?? true,
  );
}

// ══════════════════════════════════════
// EXPENSE
// ══════════════════════════════════════
class ExpenseRequest {
  final int userId;
  final double amount;
  final DateTime dateOfTransaction;
  final String? accountDeductedFrom, notes;
  final int? categoryId, currencyId;
  final bool isRecurring;

  ExpenseRequest({
    required this.userId, required this.amount, required this.dateOfTransaction,
    this.accountDeductedFrom, this.notes, this.categoryId, this.currencyId,
    this.isRecurring = false,
  });
  Map<String, dynamic> toJson() => {
    'userID': userId, 'amount': amount,
    'dateOfTransaction': dateOfTransaction.toIso8601String(),
    if (categoryId != null) 'categoryID': categoryId,
    if (accountDeductedFrom != null) 'accountDeductedFrom': accountDeductedFrom,
    if (notes != null) 'notes': notes,
    'isRecurring': isRecurring,
    if (currencyId != null) 'currencyID': currencyId,
  };
}

class ExpenseModel {
  final int expenseId;
  final double amount;
  final DateTime dateOfTransaction;
  final String? hijriDate, categoryName, accountDeductedFrom, notes, currencySymbol;
  final bool isRecurring;

  ExpenseModel({
    required this.expenseId, required this.amount, required this.dateOfTransaction,
    this.hijriDate, this.categoryName, this.accountDeductedFrom, this.notes,
    this.currencySymbol, this.isRecurring = false,
  });
  factory ExpenseModel.fromJson(Map<String, dynamic> j) => ExpenseModel(
    expenseId: j['expenseID'] ?? 0, amount: (j['amount'] as num).toDouble(),
    dateOfTransaction: DateTime.parse(j['dateOfTransaction']),
    hijriDate: j['hijriDateOfTransaction'], categoryName: j['categoryName'],
    accountDeductedFrom: j['accountDeductedFrom'], notes: j['notes'],
    currencySymbol: j['currencySymbol'], isRecurring: j['isRecurring'] ?? false,
  );
}

// ══════════════════════════════════════
// ASSET
// ══════════════════════════════════════
class AssetRequest {
  final int userId;
  final String itemName;
  final DateTime dateAcquired;
  final double purchaseValue, currentValue, quantity;
  final String? measurementUnit, notes;
  final int? assetCategoryId, currencyId;
  final bool isZakathApplicable;

  AssetRequest({
    required this.userId, required this.itemName, required this.dateAcquired,
    required this.purchaseValue, required this.currentValue,
    this.quantity = 1, this.measurementUnit, this.notes,
    this.assetCategoryId, this.currencyId, this.isZakathApplicable = true,
  });
  Map<String, dynamic> toJson() => {
    'userID': userId, 'itemName': itemName,
    'dateAcquired': dateAcquired.toIso8601String(),
    'purchaseValue': purchaseValue, 'currentValue': currentValue,
    'quantity': quantity,
    if (measurementUnit != null) 'measurementUnit': measurementUnit,
    'isZakathApplicable': isZakathApplicable,
    if (notes != null) 'notes': notes,
    if (assetCategoryId != null) 'assetCategoryID': assetCategoryId,
    if (currencyId != null) 'currencyID': currencyId,
  };
}

class AssetModel {
  final int assetId;
  final String itemName;
  final DateTime dateAcquired;
  final double purchaseValue, currentValue, quantity;
  final String? hijriDate, categoryName, measurementUnit, notes, currencySymbol;
  final bool isZakathApplicable;

  AssetModel({
    required this.assetId, required this.itemName, required this.dateAcquired,
    required this.purchaseValue, required this.currentValue,
    this.quantity = 1, this.hijriDate, this.categoryName,
    this.measurementUnit, this.notes, this.currencySymbol,
    this.isZakathApplicable = true,
  });
  factory AssetModel.fromJson(Map<String, dynamic> j) => AssetModel(
    assetId: j['assetID'] ?? 0, itemName: j['itemName'] ?? '',
    dateAcquired: DateTime.parse(j['dateAcquired']),
    purchaseValue: (j['purchaseValue'] as num).toDouble(),
    currentValue: (j['currentValue'] as num).toDouble(),
    quantity: (j['quantity'] as num).toDouble(),
    hijriDate: j['hijriDateAcquired'], categoryName: j['categoryName'],
    measurementUnit: j['measurementUnit'], notes: j['notes'],
    currencySymbol: j['currencySymbol'],
    isZakathApplicable: j['isZakathApplicable'] ?? true,
  );
}

// ══════════════════════════════════════
// ZAKATH
// ══════════════════════════════════════
class CalculateZakathRequest {
  final int userId;
  final String baseCurrency;
  final int madhabId;
  CalculateZakathRequest({required this.userId, this.baseCurrency = 'USD', this.madhabId = 1});
  Map<String, dynamic> toJson() => {'userID': userId, 'baseCurrency': baseCurrency, 'madhabID': madhabId};
}

class AssetBreakdown {
  final double cash, gold, silver, investments, otherAssets;
  AssetBreakdown({this.cash=0, this.gold=0, this.silver=0, this.investments=0, this.otherAssets=0});
  factory AssetBreakdown.fromJson(Map<String, dynamic> j) => AssetBreakdown(
    cash: (j['cash'] as num).toDouble(), gold: (j['gold'] as num).toDouble(),
    silver: (j['silver'] as num).toDouble(), investments: (j['investments'] as num).toDouble(),
    otherAssets: (j['otherAssets'] as num).toDouble(),
  );
}

class ZakathResult {
  final int calculationId;
  final DateTime calculationDate;
  final String? hijriDate, currency;
  final double totalAssets, totalLiabilities, netWorth, nisabThreshold, zakathAmount, zakathPercentage;
  final bool isZakathDue;
  final AssetBreakdown? breakdown;

  ZakathResult({
    required this.calculationId, required this.calculationDate,
    this.hijriDate, this.currency,
    required this.totalAssets, required this.totalLiabilities, required this.netWorth,
    required this.nisabThreshold, required this.zakathAmount, required this.zakathPercentage,
    required this.isZakathDue, this.breakdown,
  });
  factory ZakathResult.fromJson(Map<String, dynamic> j) => ZakathResult(
    calculationId: j['calculationID'] ?? 0,
    calculationDate: DateTime.parse(j['calculationDate']),
    hijriDate: j['hijriDate'], currency: j['currency'],
    totalAssets: (j['totalAssets'] as num).toDouble(),
    totalLiabilities: (j['totalLiabilities'] as num).toDouble(),
    netWorth: (j['netWorth'] as num).toDouble(),
    nisabThreshold: (j['nisabThreshold'] as num).toDouble(),
    zakathAmount: (j['zakathAmount'] as num).toDouble(),
    zakathPercentage: (j['zakathPercentage'] as num).toDouble(),
    isZakathDue: j['isZakathDue'] ?? false,
    breakdown: j['assetBreakdown'] != null ? AssetBreakdown.fromJson(j['assetBreakdown']) : null,
  );
}

// ══════════════════════════════════════
// PAYMENT
// ══════════════════════════════════════
class PaymentRequest {
  final int userId;
  final double paymentAmount;
  final DateTime paymentDate;
  final int? calculationId;
  final String? recipientName, recipientCategory, paymentMethod, notes;

  PaymentRequest({
    required this.userId, required this.paymentAmount, required this.paymentDate,
    this.calculationId, this.recipientName, this.recipientCategory, this.paymentMethod, this.notes,
  });
  Map<String, dynamic> toJson() => {
    'userID': userId, 'paymentAmount': paymentAmount,
    'paymentDate': paymentDate.toIso8601String(),
    if (calculationId != null) 'calculationID': calculationId,
    if (recipientName != null) 'recipientName': recipientName,
    if (recipientCategory != null) 'recipientCategory': recipientCategory,
    if (paymentMethod != null) 'paymentMethod': paymentMethod,
    if (notes != null) 'notes': notes,
  };
}

class PaymentModel {
  final int paymentId;
  final double paymentAmount;
  final DateTime paymentDate;
  final int? calculationId;
  final String? hijriDate, recipientName, recipientCategory, paymentMethod;
  final bool isVerified;

  PaymentModel({
    required this.paymentId, required this.paymentAmount, required this.paymentDate,
    this.calculationId, this.hijriDate, this.recipientName,
    this.recipientCategory, this.paymentMethod, this.isVerified = false,
  });
  factory PaymentModel.fromJson(Map<String, dynamic> j) => PaymentModel(
    paymentId: j['paymentID'] ?? 0, paymentAmount: (j['paymentAmount'] as num).toDouble(),
    paymentDate: DateTime.parse(j['paymentDate']),
    calculationId: j['calculationID'], hijriDate: j['hijriPaymentDate'],
    recipientName: j['recipientName'], recipientCategory: j['recipientCategory'],
    paymentMethod: j['paymentMethod'], isVerified: j['isVerified'] ?? false,
  );
}

// ══════════════════════════════════════
// DASHBOARD
// ══════════════════════════════════════
class DashboardStats {
  final double totalIncome, totalExpenses, totalAssets, netWorth, zakathDue, zakathPaid;
  final bool isZakathDue;
  final int daysUntilHawl;
  final String? primaryCurrency;

  DashboardStats({
    this.totalIncome=0, this.totalExpenses=0, this.totalAssets=0,
    this.netWorth=0, this.zakathDue=0, this.zakathPaid=0,
    this.isZakathDue=false, this.daysUntilHawl=0, this.primaryCurrency,
  });
  factory DashboardStats.fromJson(Map<String, dynamic> j) => DashboardStats(
    totalIncome: (j['totalIncome'] as num).toDouble(),
    totalExpenses: (j['totalExpenses'] as num).toDouble(),
    totalAssets: (j['totalAssets'] as num).toDouble(),
    netWorth: (j['netWorth'] as num).toDouble(),
    zakathDue: (j['zakathDue'] as num).toDouble(),
    zakathPaid: (j['zakathPaid'] as num).toDouble(),
    isZakathDue: j['isZakathDue'] ?? false,
    daysUntilHawl: j['daysUntilHawl'] ?? 0,
    primaryCurrency: j['primaryCurrency'],
  );
}

class RecentTransaction {
  final String type; // 'Income' | 'Expense' | 'Asset'
  final double amount;
  final DateTime date;
  final String? description, category, currencySymbol;

  RecentTransaction({
    required this.type, required this.amount, required this.date,
    this.description, this.category, this.currencySymbol,
  });
  factory RecentTransaction.fromJson(Map<String, dynamic> j) => RecentTransaction(
    type: j['type'] ?? '', amount: (j['amount'] as num).toDouble(),
    date: DateTime.parse(j['date']),
    description: j['description'], category: j['category'], currencySymbol: j['currencySymbol'],
  );
}

// ── Categories ──────────────────────────────────────────────────────────────

class CategoryModel {
  final int categoryId;
  final String? categoryNameEnglish;
  final String? categoryNameArabic;
  final String? iconName;
  final String? colorCode;

  CategoryModel({
    required this.categoryId,
    this.categoryNameEnglish,
    this.categoryNameArabic,
    this.iconName,
    this.colorCode,
  });

  factory CategoryModel.fromJson(Map<String, dynamic> j) => CategoryModel(
    categoryId:          j['categoryID'] as int,
    categoryNameEnglish: j['categoryNameEnglish'],
    categoryNameArabic:  j['categoryNameArabic'],
    iconName:            j['iconName'],
    colorCode:           j['colorCode'],
  );
}

class AssetCategoryModel {
  final int assetCategoryId;
  final String? categoryNameEnglish;
  final String? categoryNameArabic;
  final bool? zakathApplicable;
  final bool? isValuationRequired;
  final String? iconName;
  final String? description;

  AssetCategoryModel({
    required this.assetCategoryId,
    this.categoryNameEnglish,
    this.categoryNameArabic,
    this.zakathApplicable,
    this.isValuationRequired,
    this.iconName,
    this.description,
  });

  factory AssetCategoryModel.fromJson(Map<String, dynamic> j) => AssetCategoryModel(
    assetCategoryId:     j['assetCategoryID'] as int,
    categoryNameEnglish: j['categoryNameEnglish'],
    categoryNameArabic:  j['categoryNameArabic'],
    zakathApplicable:    j['zakathApplicable'],
    isValuationRequired: j['isValuationRequired'],
    iconName:            j['iconName'],
    description:         j['description'],
  );
}