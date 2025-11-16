# ABC Retail – Cloud-Powered Retail Management System

ABC Retail is a full-stack, cloud-hosted retail management platform built with **ASP.NET Core MVC**, **Azure SQL**, **Azure Functions**, and **Identity-based authentication**.  
This system provides a secure and scalable solution for managing **products, customers, carts, and orders**, with a fully implemented admin workflow and cloud-first architecture.

## Features

### ** Authentication & Role-Based Access**
- ASP.NET Core Identity for secure login/registration  
- Role separation: **Admin** and **Customer**
- Dynamic navigation bar that hides/shows menu items based thee logged-in user's role  
- Azure-hosted Identity schema

### **Cart & Order Management**
- Customers can add/remove products to the cart
- Carts are linked to the authenticated customer via UserId  
- Order placement creates a completed order with an initial status of `PENDING`
- Administrators can update order statuses to PROCESSED

### **Product, Customer & Order CRUD**
- Full CRUD for products and customers
- Admin-only access to customer management and order processing
- Clean MVC separation (Models, Views, Controllers)

### **Cloud-Native Backend**
- Azure SQL Database for persistent storage  
- **Azure Functions backend** for business logic, APIs, and background tasks  
- Frontend and backend completely separated following a modern microservices pattern  

## System Architecture

- **ASP.NET Core MVC frontend** (UI + Identity)
- **Azure Functions backend** for:
  - Order submission endpoint
  - Admin order-status update endpoint
  - Product and customer API services
- **Azure SQL Database**  
- **Azure App Service** hosting the MVC frontend
- **Azure Function App Service** hosting the application backend
- **Storage Blob Containers** for staoring product images 

This hybrid model allows:
- Loose coupling between frontend and backend
- Independent deployment pipelines
- Event-driven capability with Azure Functions

## Testing
- Unit tests for order status updates  
- Integration tests for cart operations  
- Local Function Runtime testing for Azure Functions
  
## Deployment
- Frontend deployed to Azure App Service
- Backend Azure Functions deployed via Azure Functions App
- Database hosted on Azure SQL

