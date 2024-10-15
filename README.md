# Teamwork

- Designing REST API endpoints
- Database schema
- Raw SQL queries/procedures/functions
- Workable backend server with ASP.NET Core & Entity Framework

## Topic

E-commerce backend API

---

## Requirements

_For team assignment, only 1 member in the team should fork the repo. Then, admin can invite other members to contribute in the same repo (and all the members, including admin should fork again from common repo, making PRs when changes are needed). Remember to have develop branch before merging to main. Each feature/schema/bug/issue should have it's own branch, and 1 branch/file edition should be taken by only 1 member at a time. Before making any new branch, make sure you sync the fork and run `git pull` to avoid the conflicts with the common team repo. If neccessary, check the Git lecture again._

1. Create ERD diagram with proper entities, attributes, and relationship. The ERD can be added to the project as image.
2. Design the API endpoints, following REST API architecture. The endpoints can be displayed as text with detail explaination. You can use `.md` to write the API endpoints. It is necessary to describe all the possible queries, parameters, request body, authentication (if needed), and sample response , status code for each endpoint.
3. Basic entities (Could have more if needed):

- User
- Product
- Order
- Category
- Review

4. Provide the PostgreSQL queries for the these operations:

- Functions to create one/gell all/get one/update one/delete one from products. Functions to get the most `x` purchased products (`x` would come from function parameter)
- Functions to create one/gell all/get one/update one/delete one from users
- Functions to create one/gell all/get one/update one/delete one from reviews (of all products, of a certain product, and of a certain user)
- Functions to create one/gell all/get one/update status/delete from orders
- Functions to create one/gell all/get one/update one/delete one from categories.

5. Based on your ERD and features design, continue to develop minimal backend server with CLEAN Architecture

- Each collection must have at least 20 records by the time of delivery, except products and reviews. Must have at least 200 products, and each products have at least 2 reviews.
- Consider using authentication & authorization properly
- Exception handler should be used to return meaningful response to users
- Unit testing must be done, for at least Domain and Service layers
- No deployment needed for teamwork project
- README file should descibe your teamwork with sufficient details and readable structure

## Bonus features

- Use User-defined-functions in database to to work with complex operations
- Use generics where applicable to reduce amount of replicated codes
- Backend API should allow file uploads in certain endpoints
- Keep track of users' viewed products. Write several sales analytic functions, for example:
  - Calculate the conversion rate for each product. The conversion rate is the percentage of users who viewed a product and then purchased it.
  - Calculate total sales per category in the last x months
  - Calculate the month-over-month sales for the current year
  - Calculate the percentage distribution of total sales for each category.
  - Calculate the average review rating for products within each category.

## Getting started

Here is the recommended order:

- Plan Your Database Schema before start coding

- Set Up the Project Structure

- Build the models

- Create the Repositories

- Build the Services

- Set Up Authentication & Authorization

- Build the Controllers

- Implement Error Handling Middleware
