# New Backend Endpoints - Testing & Frontend Guide

## Overview
This document describes the new endpoints added to fix the workout creation issues and improve exercise search.

---

## 1. Bulk Workout Creation (FIXED 400 ERROR)

### Endpoint
```
POST http://localhost:7009/api/workouts/bulk
```

### Description
Creates a complete workout with days and exercises in a single request. This fixes the 400 Bad Request error that was occurring with the previous approach.

### Request Body
```json
{
  "name": "Full Body Workout",
  "studentId": 1,
  "workoutDays": [
    {
      "dayOfWeek": 1,
      "exercises": [
        {
          "exerciseDbId": "0001",
          "name": "Bench Press",
          "bodyPart": "chest",
          "equipment": "barbell",
          "gifUrl": "https://example.com/exercise.gif",
          "sets": "3",
          "repetitions": "12",
          "weight": 50.0,
          "restSeconds": 90,
          "notes": "Keep elbows at 45 degrees"
        }
      ]
    },
    {
      "dayOfWeek": 3,
      "exercises": [
        {
          "exerciseDbId": "0002",
          "name": "Squats",
          "bodyPart": "legs",
          "equipment": "barbell",
          "gifUrl": "https://example.com/squat.gif",
          "sets": "4",
          "repetitions": "10",
          "weight": 80.0,
          "restSeconds": 120,
          "notes": "Depth to parallel"
        }
      ]
    }
  ]
}
```

### Response (201 Created)
```json
{
  "id": 5,
  "name": "Full Body Workout",
  "message": "Workout created successfully with all days and exercises"
}
```

### DayOfWeek Enum Values
- 0 = Sunday
- 1 = Monday
- 2 = Tuesday
- 3 = Wednesday
- 4 = Thursday
- 5 = Friday
- 6 = Saturday

---

## 2. Improved Exercise Search (PAGINATION + BETTER MATCHING)

### Endpoint
```
GET http://localhost:7009/api/exercises/search?name={searchTerm}&limit={limit}&offset={offset}
```

### Description
Search exercises by name with improved matching and pagination. Now supports partial word matching (e.g., searching "bench" will find "Barbell Bench Press", "Dumbbell Bench Press", etc.).

### Parameters
- `name` (required): Search term for exercise name
- `limit` (optional, default=30): Maximum number of results to return
- `offset` (optional, default=0): Number of results to skip (for pagination)

### Example Request
```
GET http://localhost:7009/api/exercises/search?name=bench&limit=20&offset=0
```

### Response (200 OK)
```json
{
  "data": [
    {
      "id": "0001",
      "name": "Barbell Bench Press",
      "bodyPart": "chest",
      "equipment": "barbell",
      "gifUrl": "https://v2.exercisedb.io/image/0001",
      "target": "pectorals",
      "secondaryMuscles": ["triceps", "anterior deltoid"],
      "instructions": ["Step 1...", "Step 2..."]
    }
  ],
  "total": 1,
  "limit": 20,
  "offset": 0
}
```

**Important:** Each exercise includes a `gifUrl` field that contains the GIF image URL. Use this to display exercise animations in the frontend!

---

## 3. Advanced Exercise Filter (MULTIPLE FILTERS)

### Endpoint
```
GET http://localhost:7009/api/exercises/filter?name={name}&bodyPart={bodyPart}&equipment={equipment}&target={target}&limit={limit}&offset={offset}
```

### Description
Filter exercises by multiple criteria simultaneously. All parameters are optional - you can combine any filters you want.

### Parameters
- `name` (optional): Search term for exercise name
- `bodyPart` (optional): Filter by body part (e.g., "chest", "back", "legs")
- `equipment` (optional): Filter by equipment (e.g., "barbell", "dumbbell", "body weight")
- `target` (optional): Filter by target muscle (e.g., "pectorals", "biceps")
- `limit` (optional, default=30): Maximum number of results
- `offset` (optional, default=0): Pagination offset

### Example Requests

**Search for chest exercises with barbells:**
```
GET http://localhost:7009/api/exercises/filter?bodyPart=chest&equipment=barbell&limit=20
```

**Search for "press" exercises for upper body:**
```
GET http://localhost:7009/api/exercises/filter?name=press&bodyPart=upper%20body&limit=30
```

### Response (200 OK)
```json
{
  "data": [
    {
      "id": "0001",
      "name": "Barbell Bench Press",
      "bodyPart": "chest",
      "equipment": "barbell",
      "gifUrl": "https://v2.exercisedb.io/image/0001",
      "target": "pectorals",
      "secondaryMuscles": ["triceps", "anterior deltoid"],
      "instructions": ["Step 1...", "Step 2..."]
    }
  ],
  "total": 1,
  "limit": 20,
  "offset": 0,
  "filters": {
    "name": "press",
    "bodyPart": "upper body",
    "equipment": null,
    "target": null
  }
}
```

---

## 4. Get Available Body Parts, Equipment, and Targets

These endpoints help populate filter dropdowns in the frontend.

### Get Body Parts List
```
GET http://localhost:7009/api/exercises/bodyparts
```
Returns: `["back", "cardio", "chest", "lower arms", "lower legs", "neck", "shoulders", "upper arms", "upper legs", "waist"]`

### Get Equipment List
```
GET http://localhost:7009/api/exercises/equipments
```
Returns: `["assisted", "band", "barbell", "body weight", "bosu ball", "cable", "dumbbell", ...]`

### Get Target Muscles List
```
GET http://localhost:7009/api/exercises/targets
```
Returns: `["abductors", "abs", "adductors", "biceps", "calves", ...]`

---

## Frontend Implementation Guide

### 1. Update Workout Creation Form

**Old approach (causing 400 errors):**
```typescript
// DON'T DO THIS - sends days/exercises in initial workout creation
POST /api/workouts
{
  "name": "Workout",
  "studentId": 1,
  "workoutDays": [...] // Backend doesn't accept this
}
```

**New approach (use bulk endpoint):**
```typescript
// USE THIS - dedicated bulk creation endpoint
POST /api/workouts/bulk
{
  "name": "Workout",
  "studentId": 1,
  "workoutDays": [
    {
      "dayOfWeek": 1, // Monday
      "exercises": [
        {
          "exerciseDbId": "0001",
          "name": "Bench Press",
          "bodyPart": "chest",
          "equipment": "barbell",
          "gifUrl": "https://...",
          "sets": "3",
          "repetitions": "12",
          "weight": 50,
          "restSeconds": 90,
          "notes": ""
        }
      ]
    }
  ]
}
```

### 2. Improve Exercise Search UI

**Add pagination and better filtering:**

```typescript
const [searchTerm, setSearchTerm] = useState("");
const [filters, setFilters] = useState({
  bodyPart: "",
  equipment: "",
  target: ""
});
const [page, setPage] = useState(0);
const limit = 30;

// Search with filters
const searchExercises = async () => {
  const params = new URLSearchParams({
    ...(searchTerm && { name: searchTerm }),
    ...(filters.bodyPart && { bodyPart: filters.bodyPart }),
    ...(filters.equipment && { equipment: filters.equipment }),
    ...(filters.target && { target: filters.target }),
    limit: limit.toString(),
    offset: (page * limit).toString()
  });

  const response = await fetch(
    `http://localhost:7009/api/exercises/filter?${params}`
  );
  const data = await response.json();

  // data.data contains the exercises
  // Each exercise has a gifUrl property
  return data;
};
```

### 3. Display Exercise GIFs

**Show exercise animations:**

```typescript
const ExerciseCard = ({ exercise }) => (
  <div>
    <img
      src={exercise.gifUrl}
      alt={exercise.name}
      style={{ width: '200px', height: '200px' }}
    />
    <h3>{exercise.name}</h3>
    <p>Body Part: {exercise.bodyPart}</p>
    <p>Equipment: {exercise.equipment}</p>
    <p>Target: {exercise.target}</p>
  </div>
);
```

### 4. Add Filter Dropdowns

**Populate filter options from API:**

```typescript
const [bodyParts, setBodyParts] = useState([]);
const [equipments, setEquipments] = useState([]);

useEffect(() => {
  // Load filter options
  fetch('http://localhost:7009/api/exercises/bodyparts')
    .then(res => res.json())
    .then(data => setBodyParts(data));

  fetch('http://localhost:7009/api/exercises/equipments')
    .then(res => res.json())
    .then(data => setEquipments(data));
}, []);

// Render dropdowns
<select onChange={(e) => setFilters({...filters, bodyPart: e.target.value})}>
  <option value="">All Body Parts</option>
  {bodyParts.map(bp => <option key={bp} value={bp}>{bp}</option>)}
</select>

<select onChange={(e) => setFilters({...filters, equipment: e.target.value})}>
  <option value="">All Equipment</option>
  {equipments.map(eq => <option key={eq} value={eq}>{eq}</option>)}
</select>
```

---

## Key Improvements Summary

### ✅ Fixed Issues
1. **400 Bad Request on Workout Creation** - Now use `/api/workouts/bulk` endpoint
2. **Poor Exercise Search** - Improved matching finds more relevant exercises
3. **Missing Exercise Images** - All responses include `gifUrl` field

### ✅ New Features
1. **Pagination** - Load exercises in chunks (default 30 per page)
2. **Multi-filter Search** - Combine name, body part, equipment, and target filters
3. **Better Word Matching** - Searching "bench" finds all bench press variations
4. **Filter Dropdowns** - API provides lists of valid body parts, equipment, and targets

---

## Testing Checklist for Frontend Team

- [ ] Update workout creation to use `/api/workouts/bulk`
- [ ] Test creating workout with multiple days and exercises
- [ ] Add exercise search with improved text matching
- [ ] Implement filter dropdowns (body part, equipment, target)
- [ ] Add pagination controls (limit/offset)
- [ ] Display exercise GIF images using `gifUrl` field
- [ ] Test combined filters (e.g., "chest" + "barbell" + "press")
- [ ] Verify all exercises show images properly

---

## Need Help?

If you encounter any issues or need clarification on these endpoints, please reach out!
