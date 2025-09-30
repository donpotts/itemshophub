# 🛒 ItemShopHub · AI-Powered E-Commerce Platform

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Blazor WASM](https://img.shields.io/badge/Blazor-WASM-5C2D91?logo=blazor&logoColor=white)
![Semantic Kernel](https://img.shields.io/badge/Semantic%20Kernel-enabled-1C7ED6)

A modern e-commerce platform built with Blazor WebAssembly, featuring AI-powered product assistance, comprehensive shopping cart functionality, and advanced product and service management. Fully functional with graceful AI fallbacks when credentials are unavailable.

> **Mission:** Demonstrate a complete e-commerce solution with AI integration, secure payments, order management, and comprehensive admin capabilities.

---

## 🧭 At a Glance

- 🛍️ **Complete e-commerce stack:** Products, Services, Shopping Cart, Orders, Payments, User Management
- 🧱 **Rich catalog system:** Products, Services, Brands, Categories, Features, Tags, Reviews (EF Core + SQLite)
- 🛡️ **Secure platform:** ASP.NET Core Identity with role-based authorization
- 🤖 **AI shopping assistant:** GitHub Models (OpenAI-compatible) with intelligent product recommendations
- 💳 **Payment processing:** Stripe integration for secure transactions
- 🔄 **Data management:** CSV import/export for bulk operations
- 🌓 **Modern UX:** Responsive design with dark/light theming and mobile optimization
- 📊 **Analytics dashboard:** Order tracking, review analytics, and business insights
- 🚚 **Order management:** Complete order lifecycle from cart to delivery
- 🏢 **Service marketplace:** Service companies, categories, and order management
- 📧 **Notification system:** Email notifications and in-app alerts
- 💰 **Tax & shipping:** Configurable tax rates and shipping calculations

---

## 📚 Table of Contents

1. [Quickstart](#-quickstart)
2. [Tech Stack & Dependencies](#-tech-stack--dependencies)
3. [E-Commerce Features](#-e-commerce-features)
4. [AI Configuration](#-ai-configuration)
5. [Payment Integration](#-payment-integration)
6. [Data Import / Export](#-data-import--export)
7. [Admin Features](#-admin-features)
8. [Configuration Keys](#-configuration-keys)
9. [Folder Map](#-folder-map)
10. [Key Files](#-key-files)
11. [Extensibility Ideas](#-extensibility-ideas)
12. [Troubleshooting](#-troubleshooting)
13. [Sample API Calls](#-sample-api-calls)
14. [Contact](#-contact)

---

## ⚡ Quickstart

### 1. Prerequisites

- .NET 9 SDK
- SQLite (bundled; no external install required)
- GitHub Personal Access Token (classic) with `models:read` scope for AI features
- Stripe account for payment processing (optional for testing)

### 2. Clone & Navigate

```
git clone https://github.com/donpotts/ItemShopHub.git
cd ItemShopHub
```

### 3. Configure Secrets

#### AI Configuration (Optional)
```
dotnet user-secrets init --project ItemShopHub
dotnet user-secrets set "GitHubAI:ApiKey" "ghp_xxx" --project ItemShopHub
```

#### Payment Configuration (Optional)
```
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_xxx" --project ItemShopHub
dotnet user-secrets set "Stripe:SecretKey" "sk_test_xxx" --project ItemShopHub
```

> **Note:** The platform works fully without AI or payment credentials - features gracefully degrade to basic functionality.

### 4. Apply Migrations

```
cd ItemShopHub
dotnet ef database update
```

### 5. Run the Application

```
dotnet run --project ItemShopHub
```

Visit http://localhost:5000, register an account, and explore the complete e-commerce experience.

---

## 🛠️ Tech Stack & Dependencies

### Core Framework
- **ASP.NET Core 9.0** — Web API and Identity host
- **Blazor WebAssembly** — Interactive client-side UI
- **Entity Framework Core** — Data access with SQLite provider
- **ASP.NET Core Identity** — Authentication and user management

### E-Commerce & Payments
- **Stripe.NET** — Payment processing integration
- **PDF Generation** — Order receipts and invoices
- **Email Services** — Order notifications and confirmations

### UI & Experience
- **MudBlazor** — Material Design component library
- **Bootstrap 5** — Responsive CSS framework
- **Progressive Web App** — Offline capabilities and mobile experience

### AI & Intelligence
- **Microsoft Semantic Kernel** — AI orchestration framework
- **GitHub Models** — OpenAI-compatible LLM hosting
- **Custom embedding service** — Product similarity and search

### Data & Integration
- **CsvHelper** — Bulk data import/export
- **OData** — Advanced query capabilities
- **System.Text.Json** — High-performance serialization

---

## 🛒 E-Commerce Features

### Shopping Experience
- **Product Catalog** — Browse with advanced filtering and search
- **Shopping Cart** — Persistent cart with quantity management
- **Checkout Process** — Multi-step checkout with address and payment
- **Order Management** — Complete order tracking and history
- **User Accounts** — Registration, profiles, and order history

### Product Management
- **Rich Product Data** — Name, description, pricing, images, inventory
- **Category System** — Hierarchical organization with features and tags
- **Brand Management** — Brand-based product organization
- **Review System** — Customer reviews with star ratings and analytics
- **Inventory Tracking** — Stock levels and availability status

### Service Management
- **Service Catalog** — Browse and manage service offerings
- **Service Companies** — Company profiles and service providers
- **Service Categories** — Organized service classification system
- **Service Features** — Detailed service capabilities and offerings
- **Service Tags** — Flexible service categorization and filtering
- **Service Reviews** — Customer feedback and ratings for services
- **Service Orders** — Complete service booking and fulfillment workflow
- **Service Expenses** — Track costs and expenses for service delivery

### Infrastructure & Support
- **Address Management** — Customer shipping and billing addresses
- **Notification System** — Email and in-app notification delivery
- **Tax Rates** — Configurable tax calculation by region
- **Shipping Rates** — Flexible shipping cost management
- **Image Management** — Product and service image handling

### Payment & Orders
- **Stripe Integration** — Secure card processing
- **Order Workflow** — Cart → Checkout → Payment → Fulfillment
- **PDF Receipts** — Automated invoice generation
- **Email Notifications** — Order confirmations and updates

---

## 🤖 AI Configuration

| Feature | Implementation |
|---------|----------------|
| Product Chat | Semantic Kernel with GitHub Models for intelligent product assistance |
| Search Enhancement | Embedding-based product similarity and recommendations |
| Customer Support | Context-aware responses about products and orders |
| Fallback Mode | Deterministic responses when AI is unavailable |

### AI Chat Flow
1. Product data loaded and embedded for semantic search
2. Customer questions processed through embedding similarity
3. Relevant products and context fed to chat completion
4. Responses filtered to stay within e-commerce domain
5. Graceful fallback when AI services unavailable

---

## 💳 Payment Integration

### Stripe Configuration
- Secure card processing with PCI compliance
- Support for test and live environments
- Webhook handling for payment confirmations
- Automatic receipt generation and email delivery

### Order Process
1. **Cart Management** — Add/remove items with real-time totals
2. **Checkout** — Address collection and payment method selection
3. **Payment Processing** — Secure Stripe integration
4. **Order Confirmation** — PDF receipt and email notification
5. **Order Tracking** — Status updates and fulfillment tracking

---

## 📥 Data Import / Export

CSV operations available for administrative bulk management:

| Entity | Import/Export | Key Fields |
|--------|---------------|------------|
| Products | ✅ | Name, Description, Price, InStock, Category |
| Product Reviews | ✅ | ProductId, CustomerEmail, Rating, ReviewText |
| Brands | ✅ | Name, Description, Website |
| Categories | ✅ | Name, Description, ParentCategory |
| Features | ✅ | Name, Description |
| Tags | ✅ | Name, Description |
| Services | ✅ | Name, Description, Price, ServiceCompanyId |
| Service Companies | ✅ | Name, Description, ContactInfo |
| Service Categories | ✅ | Name, Description |
| Service Features | ✅ | Name, Description |
| Service Tags | ✅ | Name, Description |
| Service Reviews | ✅ | ServiceId, CustomerEmail, Rating, ReviewText |
| Orders | ✅ | OrderDate, Status, Total, CustomerEmail |
| Service Orders | ✅ | OrderDate, Status, Total, ServiceId |

**Implementation:**
- `MudFileUpload` for client-side file handling
- `CsvHelper` for robust parsing and generation
- Bulk upsert operations with conflict resolution
- Progress indicators and detailed result reporting

---

## 👑 Admin Features

### Dashboard Analytics
- Sales metrics and revenue tracking
- Popular products and category performance
- Customer engagement and review analytics
- Order fulfillment and inventory insights

### User Management
- Role-based access control (Admin, Customer, Moderator)
- User account management and verification
- Order history and customer support tools

### Content Management
- Product catalog administration
- Review moderation and response management
- Brand and category organization
- Bulk operations and data maintenance

---

## 🔑 Configuration Keys

| Key | Description | Default |
|-----|-------------|---------|
| `GitHubAI:ApiKey` | GitHub Models PAT | (empty) → fallback mode |
| `GitHubAI:Endpoint` | AI service endpoint | `https://models.inference.ai.azure.com` |
| `GitHubAI:ChatModel` | Chat completion model | `gpt-4o-mini` |
| `GitHubAI:EmbeddingModel` | Embedding model | `text-embedding-3-small` |
| `Stripe:PublishableKey` | Stripe public key | (empty) → payment disabled |
| `Stripe:SecretKey` | Stripe private key | (empty) → payment disabled |
| `Email:SmtpServer` | Email service config | (optional) |

---

## 🗂️ Folder Map

```
ItemShopHub/                     -> Main ASP.NET Core host
  Controllers/                   -> API endpoints (Products, Orders, Payments)
  Services/                      -> Business logic (Chat, Email, PDF)
  Configuration/                 -> AI and service setup
ItemShopHub.Shared/              -> Domain models and shared logic
  Models/                        -> Entities (Product, Order, User, etc.)
ItemShopHub.Shared.Blazor/       -> Reusable UI components and pages
  Pages/                         -> Main application pages
  Components/                    -> Shared UI components
  Services/                      -> Client-side services
ItemShopHub.Blazor/              -> Blazor WASM client host
```

---

## 🔍 Key Files

### Core Application
- `Program.cs` — Application startup and service configuration
- `ApplicationDbContext.cs` — Entity Framework database context
- `SemanticKernelConfig.cs` — AI service registration and setup

### E-Commerce Logic
- `ShoppingCartController.cs` — Cart management API
- `PaymentController.cs` — Stripe payment processing
- `OrderController.cs` — Order lifecycle management
- `ProductController.cs` — Product catalog API
- `ProductReviewController.cs` — Product review management

### Service Management
- `ServiceController.cs` — Service catalog API
- `ServiceCompanyController.cs` — Service provider management
- `ServiceCategoryController.cs` — Service categorization
- `ServiceFeatureController.cs` — Service feature management
- `ServiceTagController.cs` — Service tagging system
- `ServiceReviewController.cs` — Service review management
- `ServiceOrderController.cs` — Service order workflow
- `ServiceExpenseController.cs` — Service expense tracking

### Infrastructure Services
- `AddressController.cs` — Address management
- `NotificationController.cs` — Notification system
- `EmailController.cs` — Email service integration
- `ImageController.cs` — Image upload and management
- `TaxRateController.cs` — Tax calculation rules
- `ShippingRateController.cs` — Shipping cost management

### UI Components
- `ProductCatalog.razor` — Main shopping interface
- `ShoppingCart.razor` — Cart management UI
- `Checkout.razor` — Payment and order completion
- `ProductDetails.razor` — Individual product pages
- `OrderHistory.razor` — Customer order tracking

### AI Features
- `ProductChatService.cs` — AI-powered shopping assistance
- `ProductChat.razor` — Interactive chat interface

### Business Services
- `EmailNotificationService.cs` — Email delivery and templates
- `NotificationService.cs` — Notification orchestration
- `PdfGenerationService.cs` — PDF receipt and invoice generation
- `ImageService.cs` — Image processing and optimization

---

## 🌱 Extensibility Ideas

| Enhancement | Implementation Approach |
|-------------|------------------------|
| **Multi-tenant Support** | Add tenant isolation to data models and services |
| **Advanced Analytics** | Integrate reporting dashboard with chart components |
| **Mobile App** | Leverage Blazor Hybrid for native mobile experience |
| **Inventory Management** | Add supplier integration and automated reordering |
| **Subscription Products** | Implement recurring billing with Stripe subscriptions |
| **Marketplace Features** | Multi-vendor support with commission tracking |
| **Advanced Search** | Elasticsearch integration for complex product queries |
| **Social Features** | Product wishlists, sharing, and social authentication |
| **International Support** | Multi-currency, taxation, and localization |
| **Performance Optimization** | Redis caching and CDN integration |
| **Service Scheduling** | Calendar integration for service bookings |
| **Live Chat Support** | Real-time customer support integration |
| **Loyalty Programs** | Points and rewards system |
| **Advanced Notifications** | SMS and push notification support |

---

## 🛠️ Troubleshooting

| Issue | Cause | Solution |
|-------|-------|---------|
| Payment failures | Stripe configuration | Verify API keys and webhook endpoints |
| AI responses unavailable | Missing GitHub Models key | Add valid PAT with `models:read` scope |
| Database errors | Migration issues | Run `dotnet ef database drop -f && dotnet ef database update` |
| Cart persistence issues | Local storage problems | Clear browser data and restart |
| Email notifications failing | SMTP configuration | Check email service settings |
| Performance issues | Large dataset | Implement pagination and caching strategies |

---

## 📮 Sample API Calls

**Product Search**
```http
GET /api/products?$filter=contains(Name,'laptop')&$orderby=Price
```

**Service Search**
```http
GET /api/services?$filter=contains(Name,'consulting')&$orderby=Price
```

**Add to Cart**
```http
POST /api/cart/items
{
  "productId": 123,
  "quantity": 2
}
```

**Create Service Order**
```http
POST /api/serviceorders
{
  "serviceId": 456,
  "scheduledDate": "2025-10-15",
  "customerNotes": "Prefer morning appointment"
}
```

**Process Payment**
```http
POST /api/payments/process
{
  "cartId": "cart-123",
  "paymentMethodId": "pm_xxx",
  "shippingAddress": { ... }
}
```

**AI Product Chat**
```http
POST /api/chat/products
{
  "question": "What laptops do you recommend for gaming under $1500?"
}
```

**Manage Notifications**
```http
GET /api/notifications/user/{userId}
POST /api/notifications/mark-read/{notificationId}
```

**Calculate Tax**
```http
POST /api/taxrates/calculate
{
  "amount": 100.00,
  "state": "CA",
  "zipCode": "90210"
}
```

**Get Shipping Rates**
```http
POST /api/shippingrates/calculate
{
  "destinationZipCode": "90210",
  "weight": 5.5,
  "packageType": "Box"
}
```

---

## 🤝 Contact

Questions about implementation, customization, or deployment?

**Email:** Don.Potts@DonPotts.com

Ready to build the future of e-commerce with AI-powered shopping experiences! 🚀