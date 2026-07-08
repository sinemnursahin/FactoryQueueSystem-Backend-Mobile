# API Endpoints

## Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

## Driver Shipments

- `GET /api/shipments/active`
- `POST /api/shipments/{id}/queue`
- `GET /api/shipments/{id}/status`
- `GET /api/shipments/{id}/result`

## Admin

- `GET /api/admin/vehicles/queue`
- `GET /api/admin/shipments/status/{status}`
- `POST /api/admin/shipments/{id}/call-to-scale`
- `POST /api/admin/shipments/{id}/loaded-weight`
- `POST /api/admin/shipments/{id}/start-unloading`
- `POST /api/admin/shipments/{id}/complete-unloading`
- `POST /api/admin/shipments/{id}/empty-weight`
- `GET /api/admin/shipments/completed`
