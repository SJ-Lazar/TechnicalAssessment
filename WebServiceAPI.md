# API Documentation

Base URL: `/api`

All endpoints return JSON. Unless noted otherwise, requests and responses use `application/json`.

## Users

### GET `/api/users`
- Returns all non-deleted users.
- Response: `200 OK` → `UserDto[]` with fields `id`, `email`, `active`, `deleted`, `createdAt`, `updatedAt`, `groups` (`GroupDto[]`).

### GET `/api/users/{id}`
- Returns a user by id.
- Responses: `200 OK` → `UserDto`; `404 Not Found` if the user does not exist or is deleted.

### POST `/api/users`
- Creates a user.
- Body: `{ "email": "string", "groupIds": [int]? }` (`groupIds` optional).
- Responses: `201 Created` with `Location: /api/users/{id}` and `UserDto`; `400 Bad Request` if email is missing; `409 Conflict` if email already exists for a non-deleted user.

### PUT `/api/users/{id}`
- Updates email, group memberships, and/or active status.
- Body: `{ "email": "string?", "groupIds": [int]?, "active": bool? }` (all optional; empty `groupIds` clears groups).
- Responses: `200 OK` → updated `UserDto`; `404 Not Found` if the user does not exist or is deleted; `409 Conflict` if email already in use by another active user.

### DELETE `/api/users/{id}`
- Soft-deletes the user (sets `deleted = true` and `active = false`).
- Responses: `204 No Content`; `404 Not Found` if the user does not exist or is already deleted.

### GET `/api/users/groups`
- Returns all non-deleted groups as `GroupDto[]` (`id`, `name`).

### GET `/api/users/count`
- Returns total non-deleted users.
- Response: `200 OK` → integer.

### GET `/api/users/count/active`
- Returns active, non-deleted users.
- Response: `200 OK` → integer.

### GET `/api/users/count/per-group`
- Returns user count per group name.
- Response: `200 OK` → object `{ "GroupName": count }`.

### GET `/api/users/count/group/{groupId}`
- Returns user count for a group.
- Responses: `200 OK` → integer; `404 Not Found` if the group does not exist or is deleted.

### GET `/api/users/statistics`
- Returns aggregated counts.
- Response: `200 OK` → `UserStatisticsDto` with `totalUsers`, `activeUsers`, `inactiveUsers`, `deletedUsers`, `usersPerGroup` (`{ "GroupName": count }`).

## Groups & Permissions

### GET `/api/groups`
- Returns non-deleted groups.
- Response: `200 OK` → `GroupDto[]` (`id`, `name`).

### GET `/api/groups/{id}`
- Returns group details with permissions and users.
- Responses: `200 OK` → `GroupDetailDto` (`id`, `name`, `active`, `createdAt`, `updatedAt`, `permissions` → `PermissionDto[]`, `users` → `UserDto[]` without nested groups); `404 Not Found` if missing.

### GET `/api/groups/permissions`
- Returns all non-deleted permissions sorted by name.
- Response: `200 OK` → `PermissionDto[]` (`id`, `name`).

### GET `/api/groups/{id}/available-permissions`
- Returns permissions not yet assigned to the group.
- Responses: `200 OK` → `PermissionDto[]`; `404 Not Found` if the group is missing or deleted.

### POST `/api/groups/{id}/permissions`
- Adds a permission to a group.
- Body: `{ "permissionId": int }`.
- Responses: `200 OK` → refreshed `GroupDetailDto`; `400 Bad Request` if the group or permission does not exist or the permission is already assigned.

### DELETE `/api/groups/{id}/permissions/{permissionId}`
- Removes a permission from a group.
- Responses: `200 OK` → refreshed `GroupDetailDto`; `404 Not Found` if the group is missing/deleted or the permission is not assigned.

## DTO Reference
- `UserDto`: `id`, `email`, `active`, `deleted`, `createdAt`, `updatedAt`, `groups: GroupDto[]`.
- `GroupDto`: `id`, `name`.
- `GroupDetailDto`: `id`, `name`, `active`, `createdAt`, `updatedAt`, `permissions: PermissionDto[]`, `users: UserDto[]` (no nested groups).
- `PermissionDto`: `id`, `name`.
- `UserStatisticsDto`: `totalUsers`, `activeUsers`, `inactiveUsers`, `deletedUsers`, `usersPerGroup (object)`.

## Notes
- Soft delete: users are marked `deleted = true` and `active = false`; deleted users are excluded from standard listings and counts except where explicitly noted.
- Validation: creating/updating users enforces unique email among non-deleted users.
