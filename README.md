# GitFreshSync
1. Authorize:
   - Use /api/Auth/login to generate valid token. *try admin admin*
   - Provide the generated token as Barear token.
  
2. Provide access token and api keys as a user secrets (or simmilar):
    - Example schema: {
    "Jwt:Key": "JwtKey",
    "Jwt:Issuer": "http://localhost:7114",
    "Jwt:Audience": "http://localhost:7114",
    "GitHub:Token": "YOUR_GITHUB_TOKEN",
    "Freshdesk:ApiKey": "YOUR_FRESH_DESK_API"
  }

3. Sync GitHub user to Freshdesk contact by using /api/Sync/github-to-freshdesk.

Notes: 
- GitHub user should exist
- GitHub user should have email
- GitHub user should have name
- New company would be created in you're freshdesk company list if the given one doesn't exist
- GitHub user would be created or modified in you're freshdesk contact list

To build docker image, you can use the follow command:
docker build -t your-image-name -f GitFreshSync.API/Dockerfile .
