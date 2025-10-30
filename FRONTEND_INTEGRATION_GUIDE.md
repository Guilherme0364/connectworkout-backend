# Frontend Integration Guide: ConnectWorkout API

**Last Updated:** 2025-10-29
**Backend API Version:** 1.0
**Purpose:** Complete guide for frontend developers to integrate with the ConnectWorkout backend API

---

## Table of Contents

1. [Exercise Images with RapidAPI](#exercise-images-with-rapidapi)
2. [Adding Exercises to Workouts](#adding-exercises-to-workouts)
3. [Common Errors and Solutions](#common-errors-and-solutions)
4. [Complete Example Code](#complete-example-code)

---

## Exercise Images with RapidAPI

### Image URL Format

The backend now returns **RapidAPI image URLs** with resolution parameters:

```
https://exercisedb.p.rapidapi.com/image?exerciseId={id}&resolution={resolution}
```

### Resolution Options

| Resolution | Size | Use Case |
|------------|------|----------|
| **180** | Extra Small | Thumbnails, tiny previews |
| **360** | Small | **Instructor lists** (multiple exercises visible) |
| **720** | Medium | **Student view** (single exercise detail) |
| **1080** | Large | High-quality zoom, detailed inspection |

### Image Display Strategy

#### 1. Instructor Screen (Workout Builder)
**Context:** Instructor viewing a LIST of exercises to build workouts

```javascript
// Use 360px resolution for lists
const instructorImageUrl = `https://exercisedb.p.rapidapi.com/image?exerciseId=${exercise.exerciseDbId}&resolution=360`;

// Example component
function ExerciseListItem({ exercise }) {
  return (
    <View style={styles.listItem}>
      <Image
        source={{
          uri: `https://exercisedb.p.rapidapi.com/image?exerciseId=${exercise.exerciseDbId}&resolution=360`,
          headers: {
            'x-rapidapi-key': 'YOUR_API_KEY',
            'x-rapidapi-host': 'exercisedb.p.rapidapi.com'
          }
        }}
        style={styles.thumbnail} // e.g., width: 80, height: 80
      />
      <Text>{exercise.name}</Text>
    </View>
  );
}
```

#### 2. Student Screen (Exercise Execution)
**Context:** Student viewing ONE exercise at a time to perform correctly

```javascript
// Use 720px resolution for detail view
const studentImageUrl = `https://exercisedb.p.rapidapi.com/image?exerciseId=${exercise.exerciseDbId}&resolution=720`;

// Example component
function ExerciseDetailView({ exercise }) {
  return (
    <View style={styles.detailView}>
      <Image
        source={{
          uri: `https://exercisedb.p.rapidapi.com/image?exerciseId=${exercise.exerciseDbId}&resolution=720`,
          headers: {
            'x-rapidapi-key': 'YOUR_API_KEY',
            'x-rapidapi-host': 'exercisedb.p.rapidapi.com'
          }
        }}
        style={styles.largeImage} // e.g., width: 300, height: 300
        resizeMode="contain"
      />
      <Text style={styles.title}>{exercise.name}</Text>
    </View>
  );
}
```

### Image Service Helper (Recommended)

Create a reusable service for image URLs:

```javascript
// services/exerciseImageService.js

export const RESOLUTION = {
  THUMBNAIL: 180,
  INSTRUCTOR: 360,
  STUDENT: 720,
  HD: 1080
};

export class ExerciseImageService {
  static getImageUrl(exerciseId, resolution = RESOLUTION.STUDENT) {
    // Validate resolution
    const validResolutions = [180, 360, 720, 1080];
    if (!validResolutions.includes(resolution)) {
      resolution = RESOLUTION.STUDENT; // Default to 720
    }

    return `https://exercisedb.p.rapidapi.com/image?exerciseId=${exerciseId}&resolution=${resolution}`;
  }

  static getInstructorImageUrl(exerciseId) {
    return this.getImageUrl(exerciseId, RESOLUTION.INSTRUCTOR);
  }

  static getStudentImageUrl(exerciseId) {
    return this.getImageUrl(exerciseId, RESOLUTION.STUDENT);
  }

  static getThumbnailImageUrl(exerciseId) {
    return this.getImageUrl(exerciseId, RESOLUTION.THUMBNAIL);
  }

  static getHDImageUrl(exerciseId) {
    return this.getImageUrl(exerciseId, RESOLUTION.HD);
  }

  // Get image props for React Native Image component
  static getImageProps(exerciseId, resolution, apiKey) {
    return {
      uri: this.getImageUrl(exerciseId, resolution),
      headers: {
        'x-rapidapi-key': apiKey,
        'x-rapidapi-host': 'exercisedb.p.rapidapi.com'
      }
    };
  }
}

// Usage in components:
import { ExerciseImageService, RESOLUTION } from './services/exerciseImageService';

// Instructor view
<Image source={ExerciseImageService.getImageProps(exercise.exerciseDbId, RESOLUTION.INSTRUCTOR, API_KEY)} />

// Student view
<Image source={ExerciseImageService.getImageProps(exercise.exerciseDbId, RESOLUTION.STUDENT, API_KEY)} />
```

---

## Adding Exercises to Workouts

### Endpoint

```
POST /api/workouts/{workoutId}/days/{dayId}/exercises
```

### Required Headers

```javascript
{
  'Authorization': 'Bearer YOUR_JWT_TOKEN',
  'Content-Type': 'application/json'
}
```

### Request Body Schema

**IMPORTANT:** All fields marked as required MUST be present in the request body.

```typescript
interface AddExerciseRequest {
  exerciseDbId: string;     // ✅ REQUIRED - Exercise ID from ExerciseDB API
  name: string;             // ✅ REQUIRED - Exercise name
  bodyPart: string;         // ✅ REQUIRED - e.g., "chest", "back", "legs"
  equipment: string;        // ✅ REQUIRED - e.g., "barbell", "dumbbell", "body weight"
  gifUrl: string;           // ✅ REQUIRED - Full RapidAPI image URL
  sets: string;             // ✅ REQUIRED - e.g., "3", "4"
  repetitions: string;      // ✅ REQUIRED - e.g., "10", "12", "8-12"
  weight?: number | null;   // ⚪ OPTIONAL - Weight in kg (defaults to 0)
  restSeconds?: number | null; // ⚪ OPTIONAL - Rest time in seconds (defaults to 0)
  notes?: string | null;    // ⚪ OPTIONAL - Additional notes (defaults to empty string)
}
```

### Step-by-Step: Adding an Exercise

#### Step 1: Search/Fetch Exercise from ExerciseDB

```javascript
// Example: Searching for exercises by name
async function searchExercises(searchTerm) {
  const response = await fetch(
    `http://YOUR_BACKEND_URL/api/exercises/search?name=${encodeURIComponent(searchTerm)}`,
    {
      headers: {
        'Authorization': `Bearer ${yourJwtToken}`,
        'Content-Type': 'application/json'
      }
    }
  );

  const exercises = await response.json();
  return exercises;
}

// Example response:
// [
//   {
//     "id": "0285",
//     "name": "dumbbell alternate biceps curl",
//     "bodyPart": "upper arms",
//     "equipment": "dumbbell",
//     "gifUrl": "https://exercisedb.p.rapidapi.com/image?exerciseId=0285&resolution=720",
//     "target": "biceps"
//   }
// ]
```

#### Step 2: Prepare the Request Body

**CRITICAL:** Use the EXACT data from the exercise search response.

```javascript
function prepareAddExerciseRequest(exercise, sets, repetitions, weight = null, restSeconds = 60, notes = null) {
  // ⚠️ IMPORTANT: Include ALL required fields from the exercise object
  return {
    exerciseDbId: exercise.id,              // ✅ From exercise.id
    name: exercise.name,                    // ✅ From exercise.name
    bodyPart: exercise.bodyPart,            // ✅ From exercise.bodyPart - DO NOT OMIT
    equipment: exercise.equipment,          // ✅ From exercise.equipment - DO NOT OMIT
    gifUrl: exercise.gifUrl,                // ✅ From exercise.gifUrl
    sets: sets.toString(),                  // ✅ Convert to string
    repetitions: repetitions.toString(),    // ✅ Convert to string
    weight: weight,                         // ⚪ Can be null
    restSeconds: restSeconds,               // ⚪ Can be null
    notes: notes                            // ⚪ Can be null
  };
}

// Example usage:
const exerciseToAdd = {
  id: "0285",
  name: "dumbbell alternate biceps curl",
  bodyPart: "upper arms",
  equipment: "dumbbell",
  gifUrl: "https://exercisedb.p.rapidapi.com/image?exerciseId=0285&resolution=720",
  target: "biceps"
};

const requestBody = prepareAddExerciseRequest(
  exerciseToAdd,
  "3",      // sets
  "10",     // repetitions
  null,     // weight (optional)
  60,       // restSeconds (optional)
  null      // notes (optional)
);

console.log('Request body:', JSON.stringify(requestBody, null, 2));
// Output:
// {
//   "exerciseDbId": "0285",
//   "name": "dumbbell alternate biceps curl",
//   "bodyPart": "upper arms",       ← MUST BE PRESENT
//   "equipment": "dumbbell",         ← MUST BE PRESENT
//   "gifUrl": "https://exercisedb.p.rapidapi.com/image?exerciseId=0285&resolution=720",
//   "sets": "3",
//   "repetitions": "10",
//   "weight": null,
//   "restSeconds": 60,
//   "notes": null
// }
```

#### Step 3: Send the Request

```javascript
async function addExerciseToWorkoutDay(workoutId, dayId, exerciseData, jwtToken) {
  const url = `http://YOUR_BACKEND_URL/api/workouts/${workoutId}/days/${dayId}/exercises`;

  console.log('🚀 Adding exercise to workout:', {
    workoutId,
    dayId,
    exercise: exerciseData
  });

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${jwtToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(exerciseData)
    });

    console.log('📥 Response status:', response.status);

    if (!response.ok) {
      // Get error details
      const errorData = await response.json();
      console.error('❌ Error response:', errorData);

      if (response.status === 400) {
        // Validation error - show which fields are missing/invalid
        console.error('Validation errors:', errorData.errors);
        throw new Error(`Validation failed: ${JSON.stringify(errorData.errors)}`);
      }

      throw new Error(`Failed to add exercise: ${response.status} ${response.statusText}`);
    }

    const result = await response.json();
    console.log('✅ Exercise added successfully:', result);
    return result;

  } catch (error) {
    console.error('❌ Error adding exercise:', error);
    throw error;
  }
}
```

#### Step 4: Complete Example

```javascript
// Complete workflow: Search → Select → Add

async function addExerciseWorkflow(workoutId, dayId, searchTerm, jwtToken) {
  try {
    // 1. Search for exercises
    console.log(`🔍 Searching for exercises: "${searchTerm}"`);
    const exercises = await searchExercises(searchTerm);

    if (exercises.length === 0) {
      console.log('❌ No exercises found');
      return;
    }

    console.log(`✅ Found ${exercises.length} exercises`);
    console.log('First result:', exercises[0]);

    // 2. Select the first exercise (or let user choose)
    const selectedExercise = exercises[0];

    // 3. Prepare request body with ALL required fields
    const exerciseData = {
      exerciseDbId: selectedExercise.id,
      name: selectedExercise.name,
      bodyPart: selectedExercise.bodyPart,      // ← CRITICAL: Must be included
      equipment: selectedExercise.equipment,    // ← CRITICAL: Must be included
      gifUrl: selectedExercise.gifUrl,
      sets: "3",
      repetitions: "10",
      weight: null,
      restSeconds: 60,
      notes: null
    };

    console.log('📋 Exercise data to send:', exerciseData);

    // 4. Validate that required fields are present
    const requiredFields = ['exerciseDbId', 'name', 'bodyPart', 'equipment', 'gifUrl', 'sets', 'repetitions'];
    const missingFields = requiredFields.filter(field => !exerciseData[field]);

    if (missingFields.length > 0) {
      console.error('❌ Missing required fields:', missingFields);
      throw new Error(`Missing required fields: ${missingFields.join(', ')}`);
    }

    console.log('✅ All required fields present');

    // 5. Send the request
    const result = await addExerciseToWorkoutDay(workoutId, dayId, exerciseData, jwtToken);

    console.log('✅ Exercise added successfully!');
    console.log('   Exercise ID:', result.id);
    console.log('   Exercise Name:', result.name);

    return result;

  } catch (error) {
    console.error('❌ Failed to add exercise:', error.message);
    throw error;
  }
}

// Usage:
addExerciseWorkflow(5002, 5002, "dumbbell curl", "YOUR_JWT_TOKEN");
```

---

## Common Errors and Solutions

### Error 1: 400 Bad Request - Missing Required Fields

**Symptom:**
```json
{
  "message": "One or more validation errors occurred",
  "errors": {
    "BodyPart": ["The BodyPart field is required"],
    "Equipment": ["The Equipment field is required"]
  }
}
```

**Cause:** The `bodyPart` or `equipment` fields are missing from the request body.

**Solution:**
```javascript
// ❌ WRONG - Missing bodyPart and equipment
const wrongData = {
  exerciseDbId: "0285",
  name: "dumbbell curl",
  gifUrl: "https://...",
  sets: "3",
  repetitions: "10"
};

// ✅ CORRECT - Include ALL required fields
const correctData = {
  exerciseDbId: "0285",
  name: "dumbbell curl",
  bodyPart: "upper arms",        // ← Added
  equipment: "dumbbell",         // ← Added
  gifUrl: "https://...",
  sets: "3",
  repetitions: "10"
};
```

### Error 2: 400 Bad Request - Invalid JSON

**Symptom:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400
}
```

**Cause:** The request body is not valid JSON or has syntax errors.

**Solution:**
```javascript
// ❌ WRONG - Unescaped quotes, trailing commas
const body = '{"name": "test"",, "sets": "3",}';

// ✅ CORRECT - Use JSON.stringify
const body = JSON.stringify({
  name: "test",
  sets: "3"
});
```

### Error 3: 401 Unauthorized

**Symptom:**
```json
{
  "message": "Unauthorized"
}
```

**Cause:** Missing or invalid JWT token.

**Solution:**
```javascript
// ❌ WRONG - No Authorization header
fetch(url, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  }
});

// ✅ CORRECT - Include Bearer token
fetch(url, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${jwtToken}`,
    'Content-Type': 'application/json'
  }
});
```

### Error 4: 404 Not Found - Workout Day Not Found

**Symptom:**
```json
{
  "message": "Workout day not found"
}
```

**Cause:** The `workoutId` or `dayId` doesn't exist, or they don't belong together.

**Solution:**
1. Verify the workout and day IDs are correct
2. Ensure the day belongs to the specified workout
3. Check that the coach has permission to modify this workout

### Error 5: Images Not Loading

**Symptom:** Exercise images show broken image icon or don't load.

**Causes & Solutions:**

**Cause A: Missing RapidAPI headers**
```javascript
// ❌ WRONG - No API headers
<Image source={{ uri: imageUrl }} />

// ✅ CORRECT - Include headers
<Image
  source={{
    uri: imageUrl,
    headers: {
      'x-rapidapi-key': 'YOUR_API_KEY',
      'x-rapidapi-host': 'exercisedb.p.rapidapi.com'
    }
  }}
/>
```

**Cause B: Using old URL format**
```javascript
// ❌ WRONG - Old v2.exercisedb.io URL (no longer works)
const wrongUrl = `https://v2.exercisedb.io/image/${exerciseId}`;

// ✅ CORRECT - RapidAPI URL with resolution
const correctUrl = `https://exercisedb.p.rapidapi.com/image?exerciseId=${exerciseId}&resolution=720`;
```

**Cause C: Invalid resolution parameter**
```javascript
// ❌ WRONG - Invalid resolution
const wrongUrl = `https://exercisedb.p.rapidapi.com/image?exerciseId=0001&resolution=500`;

// ✅ CORRECT - Valid resolution (180, 360, 720, or 1080)
const correctUrl = `https://exercisedb.p.rapidapi.com/image?exerciseId=0001&resolution=720`;
```

---

## Complete Example Code

### React Native Component - Instructor View

```javascript
import React, { useState, useEffect } from 'react';
import { View, Text, Image, FlatList, TouchableOpacity, StyleSheet } from 'react-native';
import { ExerciseImageService, RESOLUTION } from './services/exerciseImageService';

const RAPID_API_KEY = 'YOUR_API_KEY';
const API_BASE_URL = 'http://YOUR_BACKEND_URL';

export function InstructorExerciseList({ workoutId, dayId, jwtToken }) {
  const [exercises, setExercises] = useState([]);
  const [loading, setLoading] = useState(false);

  async function searchExercises(query) {
    setLoading(true);
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/exercises/search?name=${encodeURIComponent(query)}`,
        {
          headers: {
            'Authorization': `Bearer ${jwtToken}`,
            'Content-Type': 'application/json'
          }
        }
      );
      const data = await response.json();
      setExercises(data);
    } catch (error) {
      console.error('Error searching exercises:', error);
    } finally {
      setLoading(false);
    }
  }

  async function addExercise(exercise) {
    try {
      const exerciseData = {
        exerciseDbId: exercise.id,
        name: exercise.name,
        bodyPart: exercise.bodyPart,
        equipment: exercise.equipment,
        gifUrl: exercise.gifUrl,
        sets: "3",
        repetitions: "10",
        weight: null,
        restSeconds: 60,
        notes: null
      };

      const response = await fetch(
        `${API_BASE_URL}/api/workouts/${workoutId}/days/${dayId}/exercises`,
        {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${jwtToken}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(exerciseData)
        }
      );

      if (!response.ok) {
        const error = await response.json();
        console.error('Failed to add exercise:', error);
        alert(`Error: ${error.message}`);
        return;
      }

      const result = await response.json();
      alert(`Exercise "${result.name}" added successfully!`);

    } catch (error) {
      console.error('Error adding exercise:', error);
      alert('Failed to add exercise');
    }
  }

  function renderExerciseItem({ item }) {
    return (
      <TouchableOpacity
        style={styles.exerciseItem}
        onPress={() => addExercise(item)}
      >
        <Image
          source={ExerciseImageService.getImageProps(item.id, RESOLUTION.INSTRUCTOR, RAPID_API_KEY)}
          style={styles.exerciseThumbnail}
        />
        <View style={styles.exerciseInfo}>
          <Text style={styles.exerciseName}>{item.name}</Text>
          <Text style={styles.exerciseDetails}>
            {item.bodyPart} • {item.equipment}
          </Text>
        </View>
      </TouchableOpacity>
    );
  }

  return (
    <View style={styles.container}>
      <FlatList
        data={exercises}
        renderItem={renderExerciseItem}
        keyExtractor={(item) => item.id}
        ListEmptyComponent={
          <Text style={styles.emptyText}>
            {loading ? 'Loading...' : 'Search for exercises to add'}
          </Text>
        }
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  exerciseItem: {
    flexDirection: 'row',
    padding: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  exerciseThumbnail: {
    width: 80,
    height: 80,
    borderRadius: 8,
  },
  exerciseInfo: {
    marginLeft: 12,
    flex: 1,
    justifyContent: 'center',
  },
  exerciseName: {
    fontSize: 16,
    fontWeight: 'bold',
    marginBottom: 4,
  },
  exerciseDetails: {
    fontSize: 14,
    color: '#666',
  },
  emptyText: {
    textAlign: 'center',
    marginTop: 40,
    color: '#999',
  }
});
```

### React Native Component - Student View

```javascript
import React from 'react';
import { View, Text, Image, StyleSheet, ScrollView } from 'react-native';
import { ExerciseImageService, RESOLUTION } from './services/exerciseImageService';

const RAPID_API_KEY = 'YOUR_API_KEY';

export function StudentExerciseDetail({ exercise }) {
  return (
    <ScrollView style={styles.container}>
      <Image
        source={ExerciseImageService.getImageProps(
          exercise.exerciseDbId,
          RESOLUTION.STUDENT,
          RAPID_API_KEY
        )}
        style={styles.exerciseImage}
        resizeMode="contain"
      />

      <View style={styles.detailsContainer}>
        <Text style={styles.exerciseName}>{exercise.name}</Text>

        <View style={styles.infoRow}>
          <Text style={styles.label}>Body Part:</Text>
          <Text style={styles.value}>{exercise.bodyPart}</Text>
        </View>

        <View style={styles.infoRow}>
          <Text style={styles.label}>Equipment:</Text>
          <Text style={styles.value}>{exercise.equipment}</Text>
        </View>

        <View style={styles.workoutInfo}>
          <View style={styles.stat}>
            <Text style={styles.statValue}>{exercise.sets}</Text>
            <Text style={styles.statLabel}>Sets</Text>
          </View>

          <View style={styles.stat}>
            <Text style={styles.statValue}>{exercise.repetitions}</Text>
            <Text style={styles.statLabel}>Reps</Text>
          </View>

          {exercise.weight && (
            <View style={styles.stat}>
              <Text style={styles.statValue}>{exercise.weight} kg</Text>
              <Text style={styles.statLabel}>Weight</Text>
            </View>
          )}

          {exercise.restSeconds && (
            <View style={styles.stat}>
              <Text style={styles.statValue}>{exercise.restSeconds}s</Text>
              <Text style={styles.statLabel}>Rest</Text>
            </View>
          )}
        </View>

        {exercise.notes && (
          <View style={styles.notesContainer}>
            <Text style={styles.notesLabel}>Notes:</Text>
            <Text style={styles.notes}>{exercise.notes}</Text>
          </View>
        )}
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  exerciseImage: {
    width: '100%',
    height: 300,
    backgroundColor: '#f5f5f5',
  },
  detailsContainer: {
    padding: 20,
  },
  exerciseName: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 16,
    textTransform: 'capitalize',
  },
  infoRow: {
    flexDirection: 'row',
    marginBottom: 8,
  },
  label: {
    fontSize: 16,
    fontWeight: '600',
    marginRight: 8,
    color: '#666',
  },
  value: {
    fontSize: 16,
    textTransform: 'capitalize',
  },
  workoutInfo: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginTop: 20,
    padding: 16,
    backgroundColor: '#f8f8f8',
    borderRadius: 12,
  },
  stat: {
    alignItems: 'center',
  },
  statValue: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#007AFF',
  },
  statLabel: {
    fontSize: 12,
    color: '#666',
    marginTop: 4,
  },
  notesContainer: {
    marginTop: 20,
    padding: 16,
    backgroundColor: '#fffbea',
    borderRadius: 12,
  },
  notesLabel: {
    fontSize: 16,
    fontWeight: '600',
    marginBottom: 8,
  },
  notes: {
    fontSize: 14,
    lineHeight: 20,
  },
});
```

---

## Debugging Checklist

When adding exercises fails, check these items in order:

### 1. Request Headers
- [ ] Authorization header present with `Bearer` prefix
- [ ] Content-Type is `application/json`
- [ ] JWT token is valid and not expired

### 2. Request Body
- [ ] All required fields present: `exerciseDbId`, `name`, `bodyPart`, `equipment`, `gifUrl`, `sets`, `repetitions`
- [ ] Fields have correct data types (strings vs numbers)
- [ ] `bodyPart` and `equipment` match the exercise data exactly
- [ ] No typos in field names (case-sensitive)

### 3. URL Parameters
- [ ] `workoutId` is valid and exists
- [ ] `dayId` is valid and exists
- [ ] `dayId` belongs to `workoutId`
- [ ] User has permission to modify this workout

### 4. Backend Logs
Check the backend console for detailed error messages:
```
========== ADD EXERCISE REQUEST ==========
WorkoutId: 5002, DayId: 5002
DTO Values:
  ExerciseDbId: 0285
  Name: dumbbell alternate biceps curl
  BodyPart: upper arms
  Equipment: dumbbell
  ...
```

### 5. Network Response
- [ ] Check response status code
- [ ] Read response body for error details
- [ ] Look for validation error messages

---

## Summary

### Key Points to Remember

1. **Exercise Images**
   - Use RapidAPI URLs with resolution parameter
   - Instructor view: `resolution=360`
   - Student view: `resolution=720`
   - Include RapidAPI headers when loading images

2. **Adding Exercises**
   - Include ALL required fields in request body
   - Get `bodyPart` and `equipment` from exercise search results
   - Don't forget Authorization header with Bearer token
   - Validate request body before sending

3. **Error Handling**
   - Always check response status
   - Read error details from response body
   - Log request/response for debugging
   - Validate data before sending

### Quick Reference: Required Fields

When adding an exercise, ALWAYS include:

```javascript
{
  exerciseDbId: "from API",     // ✅ Required
  name: "from API",             // ✅ Required
  bodyPart: "from API",         // ✅ Required
  equipment: "from API",        // ✅ Required
  gifUrl: "from API",           // ✅ Required
  sets: "user input",           // ✅ Required
  repetitions: "user input",    // ✅ Required
  weight: null,                 // ⚪ Optional
  restSeconds: 60,              // ⚪ Optional
  notes: null                   // ⚪ Optional
}
```

---

## Need Help?

If you're still having issues:

1. Check the backend logs for detailed error messages
2. Verify your API key is valid and has remaining quota
3. Test the endpoint with curl or Postman first
4. Compare your request with the working examples above
5. Ensure your backend is running the latest version with the fixes

**Last Updated:** 2025-10-29
