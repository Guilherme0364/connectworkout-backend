# ConnectWorkout - Quick Reference Card

## 🚀 Quick Start

### Backend URL
```
http://YOUR_IP:7009
```

### Swagger Documentation
```
http://YOUR_IP:7009/swagger
```

---

## 🔑 Authentication

All endpoints require JWT token in header:
```http
Authorization: Bearer YOUR_JWT_TOKEN
```

---

## 📋 API Endpoints Cheat Sheet

### Coach - Workout Management

```typescript
// Get all workouts for a student
GET /api/workouts/student/{studentId}

// Get workout details
GET /api/workouts/{workoutId}

// Create workout
POST /api/workouts
{
  "studentId": 1,
  "name": "Hipertrofia Fase 1"
}

// Update workout
PUT /api/workouts/{workoutId}
{
  "name": "New Name",
  "isActive": true
}

// Delete workout
DELETE /api/workouts/{workoutId}
```

### Coach - Day Management

```typescript
// Add day to workout
POST /api/workouts/{workoutId}/days
{
  "dayOfWeek": 1  // 0=Sunday, 1=Monday, ..., 6=Saturday
}

// Delete workout day
DELETE /api/workouts/{workoutId}/days/{dayId}
```

### Coach - Exercise Management

```typescript
// Add exercise to day
POST /api/workouts/{workoutId}/days/{dayId}/exercises
{
  "exerciseDbId": "0001",
  "name": "Bench Press",
  "bodyPart": "chest",
  "equipment": "barbell",
  "gifUrl": "https://...",
  "sets": "3",
  "repetitions": "12",
  "weight": 60.5,
  "restSeconds": 90,
  "notes": "Focus on form"
}

// Update exercise
PUT /api/workouts/{workoutId}/days/{dayId}/exercises/{exerciseId}
{
  "sets": "4",
  "repetitions": "10",
  "weight": 70,
  "restSeconds": 120,
  "notes": "Increase weight"
}

// Delete exercise
DELETE /api/workouts/{workoutId}/days/{dayId}/exercises/{exerciseId}

// Reorder exercises
PUT /api/workouts/{workoutId}/days/{dayId}/exercises/reorder
{
  "exerciseIds": [3, 1, 2, 4]  // New order
}
```

### ExerciseDB - Browse Exercises

```typescript
// Search exercises
GET /api/exercises/search?name=bench

// Get exercise by ID
GET /api/exercises/{id}

// Get all body parts
GET /api/exercises/bodyparts

// Get exercises by body part
GET /api/exercises/bodypart/chest

// Get all targets
GET /api/exercises/targets

// Get exercises by target
GET /api/exercises/target/pectorals

// Get all equipment
GET /api/exercises/equipments

// Get exercises by equipment
GET /api/exercises/equipment/barbell
```

### Student - View Workouts

```typescript
// Get my workouts
GET /api/students/workouts

// Get my active workout
GET /api/students/workouts/active
```

---

## 📦 TypeScript Types Quick Reference

```typescript
// Day of week enum
export enum DayOfWeek {
  Sunday = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6,
}

// Workout summary
interface WorkoutSummary {
  id: number;
  name: string;
  createdAt: string;
  isActive: boolean;
  daysCount: number;
  exercisesCount: number;
}

// Full workout
interface Workout {
  id: number;
  name: string;
  createdAt: string;
  isActive: boolean;
  workoutDays: WorkoutDay[];
}

// Workout day
interface WorkoutDay {
  id: number;
  dayOfWeek: DayOfWeek;
  exercises: Exercise[];
}

// Exercise
interface Exercise {
  id: number;
  exerciseDbId: string;
  name: string;
  bodyPart: string;
  equipment: string;
  gifUrl: string;
  sets: string;
  repetitions: string;
  weight?: number;
  restSeconds?: number;
  order: number;
  notes: string;
}
```

---

## 🔧 Service Implementation Template

```typescript
import apiClient from '../config/api';

export const workoutService = {
  getStudentWorkouts: async (studentId: number) => {
    const response = await apiClient.get(`/workouts/student/${studentId}`);
    return response.data;
  },

  createWorkout: async (data: CreateWorkoutRequest) => {
    const response = await apiClient.post('/workouts', data);
    return response.data;
  },

  addExercise: async (workoutId: number, dayId: number, data: AddExerciseRequest) => {
    const response = await apiClient.post(
      `/workouts/${workoutId}/days/${dayId}/exercises`,
      data
    );
    return response.data;
  },
};
```

---

## ⚡ Common Code Snippets

### Load Workout Details
```typescript
const loadWorkout = async (workoutId: number) => {
  try {
    setLoading(true);
    const data = await workoutService.getWorkoutDetails(workoutId);
    setWorkout(data);
  } catch (error) {
    console.error('Error loading workout:', error);
    Alert.alert('Erro', 'Não foi possível carregar o treino');
  } finally {
    setLoading(false);
  }
};
```

### Create Workout
```typescript
const createWorkout = async (studentId: number, name: string) => {
  try {
    const result = await workoutService.createWorkout({
      studentId,
      name,
    });
    Alert.alert('Sucesso', 'Treino criado!');
    navigation.navigate('EditWorkout', { workoutId: result.id });
  } catch (error) {
    Alert.alert('Erro', 'Não foi possível criar o treino');
  }
};
```

### Add Exercise
```typescript
const addExercise = async (
  workoutId: number,
  dayId: number,
  exercise: ExerciseDbModel,
  config: { sets: string; reps: string; weight?: number }
) => {
  try {
    await workoutService.addExercise(workoutId, dayId, {
      exerciseDbId: exercise.id,
      name: exercise.name,
      bodyPart: exercise.bodyPart,
      equipment: exercise.equipment,
      gifUrl: exercise.gifUrl,
      sets: config.sets,
      repetitions: config.reps,
      weight: config.weight,
      restSeconds: 60,
      notes: '',
    });
    Alert.alert('Sucesso', 'Exercício adicionado!');
  } catch (error) {
    Alert.alert('Erro', 'Não foi possível adicionar');
  }
};
```

### Search Exercises
```typescript
const searchExercises = async (query: string) => {
  try {
    setLoading(true);
    const results = await exerciseService.searchExercises(query);
    setExercises(results);
  } catch (error) {
    Alert.alert('Erro', 'Falha na busca');
  } finally {
    setLoading(false);
  }
};
```

### Reorder Exercises
```typescript
const reorderExercises = async (newOrder: Exercise[]) => {
  const exerciseIds = newOrder.map(e => e.id);
  try {
    await workoutService.reorderExercises(workoutId, dayId, {
      exerciseIds
    });
  } catch (error) {
    Alert.alert('Erro', 'Não foi possível reordenar');
    loadExercises(); // Reload to restore original order
  }
};
```

---

## 🎨 UI Component Examples

### Workout Card
```tsx
<View style={styles.workoutCard}>
  <Text style={styles.workoutName}>{workout.name}</Text>
  {workout.isActive && (
    <View style={styles.badge}>
      <Text>ATIVO</Text>
    </View>
  )}
  <Text>{workout.daysCount} dias • {workout.exercisesCount} exercícios</Text>
</View>
```

### Exercise Card
```tsx
<View style={styles.exerciseCard}>
  <Image source={{ uri: exercise.gifUrl }} style={styles.gif} />
  <View>
    <Text style={styles.name}>{exercise.name}</Text>
    <Text>{exercise.sets} × {exercise.repetitions}</Text>
    {exercise.weight && <Text>{exercise.weight} kg</Text>}
  </View>
</View>
```

---

## 🐛 Debugging Checklist

### Authentication Issues
```typescript
// Check token exists
const token = await AsyncStorage.getItem('authToken');
console.log('Token:', token ? 'exists' : 'missing');

// Check token in request
axios.interceptors.request.use(config => {
  console.log('Headers:', config.headers);
  return config;
});
```

### Network Issues
```typescript
// Verify backend URL (not localhost!)
console.log('API URL:', API_BASE_URL);

// Test connectivity
try {
  const response = await fetch(`${API_BASE_URL}/exercises/bodyparts`);
  console.log('Status:', response.status);
} catch (error) {
  console.error('Network error:', error);
}
```

### Data Issues
```typescript
// Log response data
const workout = await workoutService.getWorkoutDetails(id);
console.log('Workout:', JSON.stringify(workout, null, 2));
```

---

## 📚 Documentation Files

| File | Description |
|------|-------------|
| `FRONTEND_INTEGRATION_GUIDE.md` | Complete frontend guide (START HERE!) |
| `EXERCISEDB_SETUP.md` | ExerciseDB API setup |
| `IMPLEMENTATION_SUMMARY.md` | Full implementation details |
| `QUICK_REFERENCE.md` | This file |

---

## 🆘 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| 401 Unauthorized | Add JWT token to Authorization header |
| CORS error | Check backend CORS config |
| Network failed | Use IP address, not localhost |
| Exercise not found | Verify exerciseDbId from API |
| Day already exists | Check existing days before adding |

---

## 📞 Getting Help

1. Check `FRONTEND_INTEGRATION_GUIDE.md` → Common Issues section
2. Review `EXERCISEDB_SETUP.md` → Troubleshooting
3. Test endpoints in Swagger UI
4. Check backend console logs
5. Verify API client configuration

---

## ✅ Implementation Checklist

### Setup
- [ ] Configure API base URL
- [ ] Set up authentication interceptor
- [ ] Create TypeScript types
- [ ] Implement workout service
- [ ] Implement exercise service

### Coach Screens
- [ ] Students list
- [ ] Workouts list
- [ ] Create workout
- [ ] Edit workout
- [ ] Manage days
- [ ] Search exercises
- [ ] Add exercise
- [ ] Edit exercise
- [ ] Reorder exercises

### Student Screens
- [ ] View workouts
- [ ] View active workout
- [ ] View day exercises

### Polish
- [ ] Error handling
- [ ] Loading states
- [ ] Empty states
- [ ] Pull to refresh
- [ ] Optimistic updates

---

**Ready to start? Open `FRONTEND_INTEGRATION_GUIDE.md`! 🚀**
