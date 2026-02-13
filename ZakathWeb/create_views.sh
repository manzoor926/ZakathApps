#!/bin/bash

# Login
cat > Views/Auth/Login.cshtml << 'EOF'
@model LoginRequest
@{ ViewData["Title"] = "Login"; }
<div class="row justify-content-center">
<div class="col-md-5"><div class="card shadow"><div class="card-body p-5">
<div class="text-center mb-4"><i class="fas fa-mosque fa-4x text-success mb-3"></i>
<h2 class="fw-bold">Zakath App</h2><p class="text-muted">Login to your account</p></div>
@if (ViewBag.Error != null) { <div class="alert alert-danger">@ViewBag.Error</div> }
<form method="post">
<div class="mb-3"><label class="form-label">Mobile Number</label>
<input asp-for="MobileNumber" class="form-control" placeholder="Enter mobile number" required /></div>
<div class="mb-3"><label class="form-label">Password</label>
<input asp-for="Password" type="password" class="form-control" placeholder="Enter password" required /></div>
<button type="submit" class="btn btn-success w-100 mb-3"><i class="fas fa-sign-in-alt me-2"></i>Login</button>
<div class="text-center"><a href="/Auth/Register" class="text-decoration-none">Don't have an account? Register</a></div>
</form></div></div></div></div>
EOF

# Register
cat > Views/Auth/Register.cshtml << 'EOF'
@model RegisterRequest
@{ ViewData["Title"] = "Register"; }
<div class="row justify-content-center"><div class="col-md-5"><div class="card shadow"><div class="card-body p-5">
<h2 class="text-center mb-4">Create Account</h2>
@if (ViewBag.Error != null) { <div class="alert alert-danger">@ViewBag.Error</div> }
<form method="post">
<div class="mb-3"><label class="form-label">Full Name</label><input asp-for="FullName" class="form-control" required /></div>
<div class="mb-3"><label class="form-label">Mobile Number</label><input asp-for="MobileNumber" class="form-control" required /></div>
<div class="mb-3"><label class="form-label">Email (Optional)</label><input asp-for="Email" type="email" class="form-control" /></div>
<div class="mb-3"><label class="form-label">Password</label><input asp-for="Password" type="password" class="form-control" required /></div>
<div class="mb-3"><label class="form-label">Confirm Password</label><input asp-for="ConfirmPassword" type="password" class="form-control" required /></div>
<button type="submit" class="btn btn-success w-100 mb-3">Register</button>
<div class="text-center"><a href="/Auth/Login">Already have an account? Login</a></div>
</form></div></div></div></div>
EOF

# Dashboard
cat > Views/Dashboard/Index.cshtml << 'EOF'
@{ ViewData["Title"] = "Dashboard"; }
<h1 class="mb-4">Dashboard</h1>
<div class="row g-4 mb-4">
@if (ViewBag.Stats != null) {
var stats = ViewBag.Stats as DashboardStats;
<div class="col-md-3"><div class="card text-white bg-primary"><div class="card-body">
<h6 class="card-title">Total Income</h6><h3>@stats.TotalIncome.ToString("C")</h3></div></div></div>
<div class="col-md-3"><div class="card text-white bg-danger"><div class="card-body">
<h6 class="card-title">Total Expense</h6><h3>@stats.TotalExpense.ToString("C")</h3></div></div></div>
<div class="col-md-3"><div class="card text-white bg-info"><div class="card-body">
<h6 class="card-title">Total Assets</h6><h3>@stats.TotalAssetValue.ToString("C")</h3></div></div></div>
<div class="col-md-3"><div class="card text-white bg-success"><div class="card-body">
<h6 class="card-title">Zakath Due</h6><h3>@stats.CurrentZakathDue.ToString("C")</h3></div></div></div>
}
</div>
<div class="card"><div class="card-body">
<h5 class="card-title mb-3">Recent Transactions</h5>
<div class="table-responsive">
<table class="table table-hover">
<thead><tr><th>Type</th><th>Amount</th><th>Date</th><th>Description</th></tr></thead>
<tbody>
@foreach (var tx in ViewBag.Transactions as List<RecentTransaction>) {
<tr><td><span class="badge bg-@(tx.Type == "Income" ? "success" : tx.Type == "Expense" ? "danger" : "info")">@tx.Type</span></td>
<td>@tx.Amount.ToString("C")</td><td>@tx.Date.ToShortDateString()</td><td>@tx.Description</td></tr>
}
</tbody>
</table>
</div>
</div></div>
EOF

# Income Index
cat > Views/Income/Index.cshtml << 'EOF'
@model List<IncomeModel>
@{ ViewData["Title"] = "Income"; }
<div class="d-flex justify-content-between align-items-center mb-4">
<h1>Income</h1><a href="/Income/Create" class="btn btn-success"><i class="fas fa-plus me-2"></i>Add Income</a>
</div>
<div class="card"><div class="card-body">
<table class="table table-hover">
<thead><tr><th>Date</th><th>Amount</th><th>Category</th><th>Description</th><th>Actions</th></tr></thead>
<tbody>
@foreach (var item in Model) {
<tr><td>@item.IncomeDate.ToShortDateString()</td><td>@item.Amount.ToString("C")</td>
<td>@item.CategoryName</td><td>@item.Description</td>
<td><form method="post" asp-action="Delete" asp-route-id="@item.IncomeId" class="d-inline">
<button type="submit" class="btn btn-sm btn-danger" onclick="return confirm('Delete this income?')">
<i class="fas fa-trash"></i></button></form></td></tr>
}
</tbody>
</table>
</div></div>
EOF

# Income Create
cat > Views/Income/Create.cshtml << 'EOF'
@model IncomeRequest
@{ ViewData["Title"] = "Add Income"; }
<div class="row justify-content-center"><div class="col-md-6">
<div class="card"><div class="card-body">
<h3 class="card-title mb-4">Add New Income</h3>
@if (ViewBag.Error != null) { <div class="alert alert-danger">@ViewBag.Error</div> }
<form method="post">
<div class="mb-3"><label class="form-label">Amount</label>
<input asp-for="Amount" type="number" step="0.01" class="form-control" required /></div>
<div class="mb-3"><label class="form-label">Date</label>
<input asp-for="IncomeDate" type="date" class="form-control" value="@DateTime.Now.ToString("yyyy-MM-dd")" required /></div>
<div class="mb-3"><label class="form-label">Category</label>
<select asp-for="CategoryId" class="form-select" required>
<option value="">Select Category</option>
@foreach (var cat in ViewBag.Categories as List<CategoryModel>) {
<option value="@cat.CategoryId">@cat.CategoryNameEnglish</option>
}
</select></div>
<div class="mb-3"><label class="form-label">Description</label>
<textarea asp-for="Description" class="form-control" rows="3"></textarea></div>
<div class="d-flex gap-2">
<button type="submit" class="btn btn-success">Save</button>
<a href="/Income" class="btn btn-secondary">Cancel</a>
</div>
</form>
</div></div>
</div></div>
EOF

# Expense Index
cat > Views/Expense/Index.cshtml << 'EOF'
@model List<ExpenseModel>
@{ ViewData["Title"] = "Expenses"; }
<div class="d-flex justify-content-between align-items-center mb-4">
<h1>Expenses</h1><a href="/Expense/Create" class="btn btn-danger"><i class="fas fa-plus me-2"></i>Add Expense</a>
</div>
<div class="card"><div class="card-body">
<table class="table table-hover">
<thead><tr><th>Date</th><th>Amount</th><th>Category</th><th>Description</th><th>Actions</th></tr></thead>
<tbody>
@foreach (var item in Model) {
<tr><td>@item.ExpenseDate.ToShortDateString()</td><td>@item.Amount.ToString("C")</td>
<td>@item.CategoryName</td><td>@item.Description</td>
<td><form method="post" asp-action="Delete" asp-route-id="@item.ExpenseId" class="d-inline">
<button type="submit" class="btn btn-sm btn-danger" onclick="return confirm('Delete this expense?')">
<i class="fas fa-trash"></i></button></form></td></tr>
}
</tbody>
</table>
</div></div>
EOF

# Expense Create
cat > Views/Expense/Create.cshtml << 'EOF'
@model ExpenseRequest
@{ ViewData["Title"] = "Add Expense"; }
<div class="row justify-content-center"><div class="col-md-6">
<div class="card"><div class="card-body">
<h3 class="card-title mb-4">Add New Expense</h3>
@if (ViewBag.Error != null) { <div class="alert alert-danger">@ViewBag.Error</div> }
<form method="post">
<div class="mb-3"><label class="form-label">Amount</label>
<input asp-for="Amount" type="number" step="0.01" class="form-control" required /></div>
<div class="mb-3"><label class="form-label">Date</label>
<input asp-for="ExpenseDate" type="date" class="form-control" value="@DateTime.Now.ToString("yyyy-MM-dd")" required /></div>
<div class="mb-3"><label class="form-label">Category</label>
<select asp-for="CategoryId" class="form-select" required>
<option value="">Select Category</option>
@foreach (var cat in ViewBag.Categories as List<CategoryModel>) {
<option value="@cat.CategoryId">@cat.CategoryNameEnglish</option>
}
</select></div>
<div class="mb-3"><label class="form-label">Description</label>
<textarea asp-for="Description" class="form-control" rows="3"></textarea></div>
<div class="d-flex gap-2">
<button type="submit" class="btn btn-success">Save</button>
<a href="/Expense" class="btn btn-secondary">Cancel</a>
</div>
</form>
</div></div>
</div></div>
EOF

echo "All views created"
