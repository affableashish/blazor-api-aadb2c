# blazor-api-aadb2c
Sign in users and call a protected API from a Blazor Server app using Azure AD B2C as the authorization server.

It's an example of Authorization Code Flow.
Read more about it [here](https://auth0.com/docs/get-started/authentication-and-authorization-flow/which-oauth-2-0-flow-should-i-use).

In this example, I'm using Azure AD B2C in place of Auth0 (shown in the diagram below). The diagram is taken from Auth0 docs.

<img width="600" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/4c73a744-2fe6-4662-850b-4dd18b5b2ebc">

In this flow we get ID token and Access token from the authorization server. We use Access Token to call the protected API.
Also helpful to know is the difference between ID token and Access token. Read all about it [here](https://auth0.com/blog/id-token-access-token-what-is-the-difference/).

Scenario [sample from Microsoft](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/4-WebApp-your-API/4-2-B2C).

## Choosing right auth library
We'll be using `Microsoft.Identity.Web` based on this flowchart.

<img width="650" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/099bacb6-e277-4fda-8394-98fab5ba846f">

[MSAL is used for fetching access tokens](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/master/1-WebApp-OIDC/1-5-B2C/README.md#where-is-msal) for accessing protected APIs (not shown here), as well as ID tokens. ASPNET Core middleware is capable of obtaining ID token on its own.

Read more about it [here](https://learn.microsoft.com/en-us/entra/msal/dotnet/getting-started/choosing-msal-dotnet).

## Create B2C tenant
Ensure that my Subscription has `Microsoft.AzureActiveDirectory` as a Resource Provider.

<img width="800" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/6c17b4a8-3583-4856-b5aa-3dbbf4277960">

Go to Subscription -> Resource Providers

<img width="800" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/544b23db-db5f-4e8e-8a85-e0e433126e84">

Select the row -> Register

<img width="300" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/46614e39-6b26-4837-866b-626faa0be370">
<br>
<img width="650" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/66377321-2770-4561-a117-5fd8dcacf05a">

### Create Tenant
Create a new Azure AD B2C tenant in your subscription (if you don't already have one).  
Before my app can interact with Azure AD B2C, it must be registered in a tenant that I manage.

Create an Azure AD B2C tenant. Instructions [here](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant?WT.mc_id=Portal-Microsoft_AAD_B2CAdmin).

Fill up the form

<img width="500" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/36ab418f-bb9a-41be-923d-6e89edfe207d">

You can see the directory now

<img width="850" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/a4b275ef-d778-4dc1-8b0b-97ef662748a9">

Add this new directory that you just created to favorites by searching for Azure AD B2C and clicking on the star.

<img width="600" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/ce51946f-8026-4d6a-bd0e-edb23d174bcf">

Now it'll show up like this:

<img width="225" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/b6853eae-c0cd-474b-9254-bbbc25ccadbf">

Now, switch to your directory:

<img width="800" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/0701cfd1-059c-4e1b-ace1-89fc8125fddb">

## Register Web App in Azure AD B2C
### Register web app
Follow instructions [here](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-register-applications?tabs=app-reg-ga).

Fill up the form

<img width="600" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/00994aea-e863-4012-908b-c5b43364744a">

Add one more Redirect Uri using info from `launchSettings.json`

<img width="750" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/55d378cf-269b-4c3a-994d-ea1099d08be6">

This 👇

<img width="250" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/4e9b4b02-7757-4651-9c95-af026c160c25">

Record client Id: 171b3d8f-8ff1-48b7-a5be-31b0413944ee

### Create client secret
For this web app we just registered, we need to create an application secret. This is also known as application password. Our app will  exchange authorization code (our app receives this from auth server when user authenticates and consent. See pic from the section where I talk about Oauth flows for more info) + client Id + client secret for an Access Token.

App Registrations page -> Munson Web -> Certificates and secrets -> New Client Secret

<img width="750" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/c43f1b2c-c2f9-4663-b5b1-ce6aca1a6093">

Record value: fA58Q~6MzNJ3yk.YTq9iP51R1niJFWuxaGxTIcub

### Enable Id tokens and Access tokens
If we register this app and configure it with jwt.ms for testing a user flow or custom policy, we need to enable implicit grant flow in the app registration.

Authentication -> Select both options -> Save

<img width="650" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/4c4c1746-36a3-4b1b-b76e-bdf4503fa7f1">

### Create User Flows
[Reference](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-user-flow)

A user flow lets us determine how users interact with our application when they do things like sign-in, sign-up, edit a profile or reset a password.

#### Create sign up and sign in flows
Select Azure AD B2C -> User Flows (Under Policies) -> New User flow

Create a User flow -> Sign up and sign in -> Recommended (Under Version)  -> Create

Name: B2C_1_ ----> SignUpSignIn

<img width="850" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/9d4f5671-aba1-4cee-aff8-e7202db2bb0d">

-> Create

#### Test it
Open the user flow you just created -> Run user flow

<img width="375" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/e886816e-f76e-4f46-b011-c9ccbb7efd4b">

I run the flow now and get this ID token back

<img width="750" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/56dc0d84-697d-4885-9d7c-d70d839548a4">

#### Enable self service password reset
Select the SignUpSignIn user flow that we just created -> Properties -> Self service password reset -> Save

<img width="750" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/abff6d7f-a5a7-48ca-981e-25a01eead89a">

#### Enable Self Service Profile Editing
User flows -> New user flow

Create a user flow -> Profile editing -> Recommended -> Create

<img width="850" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/472f1499-b273-4e5e-b8ae-2e3a020dedb7">

#### Test this flow
Login using the credentials you used earlier -> You'll see a page to update your display name and job title.

<img width="300" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/91d3e0cb-68b0-4d09-8537-4130b5ef623c">

You'll get back the token:

<img width="650" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/aed0ce68-badd-4968-8e13-46d92d09a6bc">

## Register Web API in Azure AD B2C
App Registration -> New Registration

<img width="650" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/4e37b9ce-435a-4390-8d14-4a31e62a4779">

Record the client ID: 2d491ecb-81e4-40a2-abbb-659c2484303a

### Configure Web API app scopes
Do a bit of reading [here](https://auth0.com/blog/permissions-privileges-and-scopes/) first.

Permission is a declaration of an action that can be executed on a resource.  
Resources (For eg: your web API) expose **permissions**.

Expose an API -> Application ID URI -> Add -> Change the GUID to more human readable like: munson-api

<img width="800" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/26ce2dac-6697-4359-847f-0d92028ab906">

Add Scopes: read and write

<img width="350" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/0a86ed52-2701-43b3-a88b-59f3eee551f7">
<br>
**Note:**  
Scopes enable a mechanism to define what an application can do on behalf of the user.  
Scopes are permissions of a resource (scopes are exposed in API so API is the resource here) that the application wants to exercise on behalf of the user. These permissions are in Web app.

### Grant the web app permissions to the web API
App Registrations -> Munson Web -> API Permissions -> Add a permission

-> APIs my organization uses
-> Munson API (This is the API to which the web app should be granted access)
-> Select Permissions: read and write
-> Add permissions
<img width="900" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/f1e636bd-ec9a-4904-8a2a-26ccba62d78c">

-> Grant admin consent for Munson Pickles (Munson Pickles is my Tenant/ Directory name)

Now it should look like this (web app now has permissions to call Microsoft Graph and Munson API)

<img width="850" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/64286de7-62c0-43b6-9343-0f7a83d9364e">

Copy the scope names:  
https://munsonpickles3.onmicrosoft.com/munson-api/read  
https://munsonpickles3.onmicrosoft.com/munson-api/write

**Note:**  
On the resource side, user's privileges must be checked even in the presence of granted scopes.

<img width="850" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/0029c399-1950-4d12-8141-7250ea0233d4">

Whenever the client Blazor Server app calls the API, the logged in user will grant "read" access to the Blazor Server app which will present that scope to the backend API which is protected by a policy that requires "read" scope. In this case the consent screen will not appear to the user because these scopes are only consented by the admin and we did that consent already with this: _-> Grant admin consent for Munson Pickles_. 

## Setup the API project to use Azure AD B2C
[Reference](https://learn.microsoft.com/en-us/azure/active-directory-b2c/enable-authentication-web-api?tabs=csharpclient)

### Setup appsettings.json
Azure Ad B2C Instance Name [Hint](https://jamescook.dev/azure-b2c-getting-started): 
The first part of your Azure B2C tenant domain name combined with `b2clogin.com`. It should look like `mydomain.b2clogin.com`.

<img width="450" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/05c9d507-f1c9-41ec-8829-63d40fe5c77f">

The appsettings.json should look like this:

<img width="450" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/059cb3e0-42ba-4947-9737-bb5aa35c67ce">

### Add required packages
```
Microsoft.Identity.Web
```
It parses the HTTP authentication header, validates the token and extracts claims.

Add it to `Program.cs`
```
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
```

Set it up with steps outlined in the referenced page above. **👉Even better, just take a look at the code.👈**

## Setup the Web App project to use Azure AD B2C
[Reference](https://learn.microsoft.com/en-us/azure/active-directory-b2c/enable-authentication-web-application?tabs=visual-studio)

Create a Blazor web app with No auth

<img width="500" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/df58dc85-c123-4488-9f66-df4e84a1305d">

### Setup appsettings.json
<img width="500" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/7808c930-70b4-4362-ba5c-500040055a08">

### Add required packages
```
Microsoft.Identity.Web
Microsoft.Identity.Web.UI
Microsoft.Identity.Web.DownstreamApi
```
The Microsoft Identity Web library sets up the authentication pipeline with cookie-based authentication. It takes care of sending and receiving HTTP authentication messages, token validation, claims extraction, and more.

Set it up with steps outlined in the referenced page above. **👉Even better, just take a look at the code.👈**

### Wrap your Router in App.razor with CascadingAuthenticationState
Also replace `RouteView` with `AuthorizeRouteView`.

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)"/>
            <FocusOnNavigate RouteData="@routeData" Selector="h1"/>
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```
### Add LoginDisplay
Add a `LoginDisplay.razor` component in Shared folder and use that in `MainLayout.razor`.

<img width="300" alt="image" src="https://github.com/affableashish/blazor-api-aadb2c/assets/30603497/6276be99-13c4-4ffe-8a0f-6ee2d2364647">

## Look at the code to see it all in action 🤓
Jump into the code to see how the client calls the protected API. 

[Reference](https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-call-api-app-configuration?tabs=aspnetcore)
