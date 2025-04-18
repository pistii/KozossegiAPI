
### **Backend – ASP.NET Core 7 + MySQL**

```md
# KozossegiAPI

This API serves the frontend: [kozossegi frontend](https://github.com/pistii/kozossegi).


##  Stack

- ASP.NET Core 7
- Entity Framework Core
- MySQL database
- SignalR (realtime communication)
- JWT Authentication
- Hangfire (for background tasks)
- REST API endpoints

 # Main modules

- User authentication (JWT)
- Post, Comment, Like, dislike
- Communication (in realtime with SignalR)
- Notifications (with automatic and key triggers)
- Profile and settings
- AI chatbot integration prepared.
- Image upload, handle media URLs

## Database
The MySql database well structured in tables and relations. The (2024.05.15) state:
![Database connection](https://github.com/pistii/KozossegiAPI/blob/master/dbsetup.png?raw=true)

## Integrated functions
- Message sending in real time.
- Comments, Posts
- Notifications (birthday, friend request in real time)
- Avatar upload both locally and into cloud.
- Image uploading prepared and under relocation.
- SignalR based communication
- JWT middleware
- Load balancing

## API endpoints

Every endpoint used with `api` prefix:
- `POST api/users/authenticate` – login
- `GET api/notification/getAll/` – request notifications
- `GET api/post/getAll/` – request posts

# Next steps
- Microservice architecture (New Repository)
- gRPC + Kafka + Google Cloud Run
- MongoDb + Redis cache integration
- AI bots, background processes are more modularized.
