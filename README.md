# WexTransaction

# Requirements

## Requirement #1: Store a Purchase Transaction

Your application must be able to accept and store (i.e., persist) a purchase transaction with a description, transaction
date, and a purchase amount in **United States dollars**. When the transaction is stored, it will be assigned a **unique identifier**.

## Field requirements

● Description: must not exceed 50 characters
● Transaction date: must be a valid date format
● Purchase amount: must be a valid positive amount rounded to the nearest cent
● Unique identifier: must uniquely identify the purchase

## Requirement #2: Retrieve a Purchase Transaction in a Specified Country’s Currency

Provide a way to retrieve the stored purchase transactions converted to currencies supported by the **Treasury Reporting Rates of Exchange API**
based upon the exchange rate active for the date of the purchase.

**https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange**

The retrieved purchase should include the identifier, the description, the transaction date, the original US dollar purchase
amount, the exchange rate used, and the converted amount based upon the specified currency’s exchange rate for the
date of the purchase.

## Currency conversion requirements

● When converting between currencies, you do not need an exact date match, but must use a currency conversion rate 
    less than or equal to the purchase date from within the last 6 months.
● If no currency conversion rate is available within 6 months equal to or before the purchase date, an error should be 
    returned stating the purchase cannot be converted to the target currency.
● The converted purchase amount to the target currency should be rounded to two decimal places (i.e., cent).

## General Structure

- [x] The project will use **OpenSpec** as its AI tool.
- [x] The project will use Clean Architecture as its design architecture.
- [x] The project will be divided into layers, adhering to the architectural design.
- [x] There is no information regarding the volume of requests, but the application will be prepared.
- [x] Build the **Domain** layer.
- [ ] Build the **Application** layer.
- [ ] Build the **Infrastructure** layer.
- [ ] Build the **Presentation** layer.


## OpenSpec

-   I used OpenSpec to help me create this application. I employed the Spec-Driven Development methodology to build the application, refining each step of the project structure whenever possible.
-   After that, I chose Clean Architecture and the SOLID principles as the design architecture.
-   Also i created a **SKILL** to review each step that I advanced