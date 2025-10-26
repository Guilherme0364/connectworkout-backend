# Student Dashboard & Exercise Visual Content Guide

This guide covers two new implementations:
1. Student Dashboard Endpoint
2. Exercise Visual Content Display

---

## Part 1: Student Dashboard Endpoint

### Overview

A new endpoint provides aggregated student dashboard data in a single API call.

**Endpoint:** `GET /api/students/dashboard`
**Authentication:** Required (JWT Bearer token)
**Access:** Students only

---

### Request

```http
GET /api/students/dashboard HTTP/1.1
Host: localhost:7009
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

### Response Format

```json
{
  "studentName": "João Silva",
  "hasTrainer": true,
  "currentTrainer": {
    "id": 1,
    "name": "Carlos Personal",
    "email": "carlos@example.com",
    "description": "10 anos de experiência em musculação",
    "studentCount": 15
  },
  "workoutCount": 3,
  "activeWorkoutId": 5,
  "pendingRequestsCount": 0
}
```

### Response Fields

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `studentName` | `string` | No | Authenticated student's name |
| `hasTrainer` | `boolean` | No | Whether student has assigned trainer |
| `currentTrainer` | `object` | Yes | Most recent trainer info (null if no trainer) |
| `currentTrainer.id` | `int` | No | Trainer's ID |
| `currentTrainer.name` | `string` | No | Trainer's name |
| `currentTrainer.email` | `string` | No | Trainer's email |
| `currentTrainer.description` | `string` | Yes | Trainer's bio/description |
| `currentTrainer.studentCount` | `int` | No | Total students this trainer has |
| `workoutCount` | `int` | No | Total workouts assigned to student |
| `activeWorkoutId` | `int` | Yes | ID of active workout (null if none) |
| `pendingRequestsCount` | `int` | No | Pending connection requests (always 0 for now) |

---

### Business Logic

#### Current Trainer Selection
- Selects **most recent** instructor by `ConnectedAt` date (DESC)
- If student has multiple trainers, returns the latest one
- If no trainer: `hasTrainer = false`, `currentTrainer = null`

#### Active Workout Selection
- Selects **most recent** active workout by `CreatedAt` date (DESC)
- Filters: `IsActive = true` AND `StudentId = authenticated user`
- If no active workout: `activeWorkoutId = null`

#### Trainer Student Count
- Counts all students assigned to trainer
- Query: `COUNT(*) FROM StudentInstructor WHERE InstructorId = X`

#### Pending Requests
- Currently hardcoded to `0`
- Future implementation requires separate request tracking table

---

### Usage Examples

#### cURL

```bash
# 1. Login first
curl -X POST http://localhost:7009/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "student@example.com",
    "password": "Password123"
  }'

# 2. Copy the accessToken from response, then:
curl -X GET http://localhost:7009/api/students/dashboard \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json"
```

#### React Native / TypeScript

```typescript
// Type Definitions
interface InstructorSummary {
  id: number;
  name: string;
  email: string;
  description: string;
  studentCount: number;
}

interface StudentDashboard {
  studentName: string;
  hasTrainer: boolean;
  currentTrainer: InstructorSummary | null;
  workoutCount: number;
  activeWorkoutId: number | null;
  pendingRequestsCount: number;
}

// Fetch Function
const fetchDashboard = async (): Promise<StudentDashboard> => {
  const token = await AsyncStorage.getItem('accessToken');

  if (!token) {
    throw new Error('No authentication token found');
  }

  const response = await fetch('http://YOUR_API_URL:7009/api/students/dashboard', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Token expired');
    }
    throw new Error('Failed to fetch dashboard');
  }

  return await response.json();
};

// Component Usage
const DashboardScreen = () => {
  const [dashboard, setDashboard] = useState<StudentDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        setLoading(true);
        const data = await fetchDashboard();
        setDashboard(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    loadDashboard();
  }, []);

  if (loading) return <ActivityIndicator />;
  if (error) return <Text>Error: {error}</Text>;
  if (!dashboard) return null;

  return (
    <ScrollView style={styles.container}>
      {/* Welcome Section */}
      <View style={styles.header}>
        <Text style={styles.welcomeText}>Bem-vindo,</Text>
        <Text style={styles.studentName}>{dashboard.studentName}!</Text>
      </View>

      {/* Trainer Card */}
      {dashboard.hasTrainer && dashboard.currentTrainer ? (
        <View style={styles.trainerCard}>
          <Text style={styles.cardTitle}>Seu Personal Trainer</Text>
          <View style={styles.trainerInfo}>
            <View style={styles.trainerAvatar}>
              <Text style={styles.avatarText}>
                {dashboard.currentTrainer.name.charAt(0)}
              </Text>
            </View>
            <View style={styles.trainerDetails}>
              <Text style={styles.trainerName}>
                {dashboard.currentTrainer.name}
              </Text>
              <Text style={styles.trainerEmail}>
                {dashboard.currentTrainer.email}
              </Text>
              <Text style={styles.trainerDesc}>
                {dashboard.currentTrainer.description}
              </Text>
              <Text style={styles.studentCount}>
                {dashboard.currentTrainer.studentCount} alunos
              </Text>
            </View>
          </View>
          <TouchableOpacity
            style={styles.contactButton}
            onPress={() => {/* Contact trainer */}}
          >
            <Icon name="mail-outline" size={20} color="#fff" />
            <Text style={styles.contactButtonText}>Enviar Mensagem</Text>
          </TouchableOpacity>
        </View>
      ) : (
        <View style={styles.noTrainerCard}>
          <Icon name="person-add-outline" size={48} color="#ccc" />
          <Text style={styles.noTrainerText}>
            Você ainda não tem um personal trainer
          </Text>
          <TouchableOpacity
            style={styles.findTrainerButton}
            onPress={() => {/* Navigate to find trainer */}}
          >
            <Text style={styles.findTrainerText}>Encontrar Trainer</Text>
          </TouchableOpacity>
        </View>
      )}

      {/* Stats Cards */}
      <View style={styles.statsContainer}>
        <View style={styles.statCard}>
          <Icon name="fitness-outline" size={32} color="#007AFF" />
          <Text style={styles.statNumber}>{dashboard.workoutCount}</Text>
          <Text style={styles.statLabel}>Treinos</Text>
        </View>

        <View style={styles.statCard}>
          <Icon name="checkmark-circle-outline" size={32} color="#4CAF50" />
          <Text style={styles.statNumber}>
            {dashboard.activeWorkoutId ? '1' : '0'}
          </Text>
          <Text style={styles.statLabel}>Ativo</Text>
        </View>

        <View style={styles.statCard}>
          <Icon name="time-outline" size={32} color="#FF9800" />
          <Text style={styles.statNumber}>
            {dashboard.pendingRequestsCount}
          </Text>
          <Text style={styles.statLabel}>Pendentes</Text>
        </View>
      </View>

      {/* Active Workout CTA */}
      {dashboard.activeWorkoutId && (
        <TouchableOpacity
          style={styles.activeWorkoutButton}
          onPress={() => navigation.navigate('WorkoutDetail', {
            workoutId: dashboard.activeWorkoutId
          })}
        >
          <Icon name="play-circle" size={24} color="#fff" />
          <Text style={styles.activeWorkoutText}>
            Começar Treino Ativo
          </Text>
        </TouchableOpacity>
      )}
    </ScrollView>
  );
};

// Styling
const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  header: {
    backgroundColor: '#007AFF',
    padding: 24,
    paddingTop: 48,
  },
  welcomeText: {
    fontSize: 16,
    color: 'rgba(255,255,255,0.8)',
  },
  studentName: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#fff',
    marginTop: 4,
  },
  trainerCard: {
    backgroundColor: '#fff',
    margin: 16,
    padding: 20,
    borderRadius: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: '600',
    color: '#333',
    marginBottom: 16,
  },
  trainerInfo: {
    flexDirection: 'row',
    marginBottom: 16,
  },
  trainerAvatar: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: '#007AFF',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 16,
  },
  avatarText: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#fff',
  },
  trainerDetails: {
    flex: 1,
  },
  trainerName: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
    marginBottom: 4,
  },
  trainerEmail: {
    fontSize: 14,
    color: '#666',
    marginBottom: 8,
  },
  trainerDesc: {
    fontSize: 13,
    color: '#888',
    marginBottom: 8,
  },
  studentCount: {
    fontSize: 12,
    color: '#007AFF',
    fontWeight: '600',
  },
  contactButton: {
    flexDirection: 'row',
    backgroundColor: '#007AFF',
    padding: 12,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 8,
  },
  contactButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  noTrainerCard: {
    backgroundColor: '#fff',
    margin: 16,
    padding: 32,
    borderRadius: 12,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  noTrainerText: {
    fontSize: 16,
    color: '#666',
    textAlign: 'center',
    marginTop: 16,
    marginBottom: 20,
  },
  findTrainerButton: {
    backgroundColor: '#007AFF',
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
  },
  findTrainerText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  statsContainer: {
    flexDirection: 'row',
    paddingHorizontal: 16,
    gap: 12,
    marginBottom: 16,
  },
  statCard: {
    flex: 1,
    backgroundColor: '#fff',
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  statNumber: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#333',
    marginTop: 8,
  },
  statLabel: {
    fontSize: 12,
    color: '#666',
    marginTop: 4,
  },
  activeWorkoutButton: {
    flexDirection: 'row',
    backgroundColor: '#4CAF50',
    margin: 16,
    padding: 16,
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 8,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.2,
    shadowRadius: 4,
    elevation: 4,
  },
  activeWorkoutText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: 'bold',
  },
});
```

---

### Error Handling

#### HTTP Status Codes

| Code | Meaning | Response |
|------|---------|----------|
| 200 | Success | Dashboard data |
| 401 | Unauthorized | `{ "message": "User not authenticated" }` |
| 403 | Forbidden | User is not a student |
| 500 | Server Error | `{ "message": "Error retrieving dashboard data" }` |

#### Error Handling Implementation

```typescript
const handleDashboardError = (error: any, navigation: any) => {
  if (error.message === 'Token expired') {
    Alert.alert(
      'Sessão Expirada',
      'Faça login novamente para continuar',
      [
        {
          text: 'OK',
          onPress: () => {
            AsyncStorage.removeItem('accessToken');
            navigation.navigate('Login');
          }
        }
      ]
    );
  } else if (error.status === 403) {
    Alert.alert(
      'Acesso Negado',
      'Você não tem permissão para acessar este recurso'
    );
  } else {
    Alert.alert(
      'Erro',
      'Não foi possível carregar seus dados. Tente novamente.'
    );
  }
};
```

---

## Part 2: Exercise Visual Content Display

### Overview

The exercise model already includes visual content fields. This guide shows how to display them effectively in your app.

### Available Visual Data

#### From Exercise Entity (Stored in Database)

```typescript
interface ExerciseBasic {
  id: number;
  exerciseDbId: string;  // Reference to ExerciseDB
  name: string;
  bodyPart: string;      // "chest", "back", "legs", etc.
  equipment: string;     // "barbell", "dumbbell", "body weight"
  gifUrl: string;        // ⭐ Main visual content (GIF animation)
  sets: string;
  repetitions: string;
  weight: number | null;
  restSeconds: number | null;
  order: number;
  notes: string | null;  // Instructor notes
}
```

#### From ExerciseDB API (Full Details)

```typescript
interface ExerciseFullDetails {
  id: string;
  name: string;
  bodyPart: string;
  equipment: string;
  gifUrl: string;              // ⭐ GIF animation
  target: string;              // Primary muscle (e.g., "pectorals")
  secondaryMuscles: string[];  // ["deltoids", "triceps"]
  instructions: string[];      // ⭐ Step-by-step guide
}
```

### Endpoint to Get Full Visual Details

```http
GET /api/exercises/{exerciseDbId}
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "id": "0001",
  "name": "3/4 Sit-Up",
  "bodyPart": "waist",
  "target": "abs",
  "equipment": "body weight",
  "gifUrl": "https://exercisedb.p.rapidapi.com/exercises/0001.gif",
  "secondaryMuscles": ["hip flexors", "lower back"],
  "instructions": [
    "Lie flat on your back with your knees bent and feet flat on the ground.",
    "Place your hands behind your head with your elbows pointing outwards.",
    "Engaging your abs, slowly lift your upper body off the ground.",
    "Pause for a moment at the top.",
    "Slowly lower your upper body back down to the starting position."
  ]
}
```

---

### Visual Content Display Implementation

#### Exercise Detail Screen with Visual Content

```tsx
import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  Image,
  ScrollView,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Dimensions
} from 'react-native';

const { width } = Dimensions.get('window');

interface ExerciseDetailScreenProps {
  route: {
    params: {
      exerciseId: number;
      exerciseDbId: string;
    };
  };
}

const ExerciseDetailScreen: React.FC<ExerciseDetailScreenProps> = ({ route }) => {
  const { exerciseId, exerciseDbId } = route.params;

  const [exercise, setExercise] = useState<any>(null);
  const [fullDetails, setFullDetails] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'overview' | 'instructions'>('overview');

  useEffect(() => {
    const fetchExerciseDetails = async () => {
      try {
        setLoading(true);
        const token = await AsyncStorage.getItem('accessToken');

        // Fetch basic exercise data from workout
        const workoutResponse = await fetch(
          `http://YOUR_API_URL:7009/api/workouts/${workoutId}`,
          { headers: { 'Authorization': `Bearer ${token}` }}
        );
        const workout = await workoutResponse.json();

        // Find this specific exercise
        const foundExercise = workout.workoutDays
          .flatMap(day => day.exercises)
          .find(ex => ex.id === exerciseId);

        setExercise(foundExercise);

        // Fetch full details from ExerciseDB
        const detailsResponse = await fetch(
          `http://YOUR_API_URL:7009/api/exercises/${exerciseDbId}`,
          { headers: { 'Authorization': `Bearer ${token}` }}
        );

        if (detailsResponse.ok) {
          const details = await detailsResponse.json();
          setFullDetails(details);
        }
      } catch (error) {
        console.error('Error fetching exercise:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchExerciseDetails();
  }, [exerciseId, exerciseDbId]);

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#007AFF" />
      </View>
    );
  }

  if (!exercise) return null;

  return (
    <ScrollView style={styles.container}>
      {/* ===== VISUAL CONTENT: GIF Animation ===== */}
      <View style={styles.gifContainer}>
        <Image
          source={{ uri: exercise.gifUrl }}
          style={styles.exerciseGif}
          resizeMode="contain"
          defaultSource={require('./assets/exercise-placeholder.png')}
        />
      </View>

      {/* Exercise Name */}
      <View style={styles.headerSection}>
        <Text style={styles.exerciseName}>{exercise.name}</Text>
      </View>

      {/* ===== VISUAL CONTENT: Muscle Groups ===== */}
      {fullDetails && (
        <View style={styles.muscleSection}>
          <Text style={styles.sectionTitle}>Músculos Trabalhados</Text>

          <View style={styles.muscleCard}>
            <View style={styles.musclePrimary}>
              <Icon name="fitness" size={20} color="#007AFF" />
              <View style={styles.muscleInfo}>
                <Text style={styles.muscleLabel}>Principal</Text>
                <Text style={styles.muscleValue}>{fullDetails.target}</Text>
              </View>
            </View>

            {fullDetails.secondaryMuscles?.length > 0 && (
              <View style={styles.muscleSecondary}>
                <Icon name="body" size={20} color="#666" />
                <View style={styles.muscleInfo}>
                  <Text style={styles.muscleLabel}>Secundários</Text>
                  <Text style={styles.muscleValue}>
                    {fullDetails.secondaryMuscles.join(', ')}
                  </Text>
                </View>
              </View>
            )}
          </View>
        </View>
      )}

      {/* Tab Selector */}
      <View style={styles.tabContainer}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'overview' && styles.tabActive]}
          onPress={() => setActiveTab('overview')}
        >
          <Text style={[
            styles.tabText,
            activeTab === 'overview' && styles.tabTextActive
          ]}>
            Visão Geral
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.tab, activeTab === 'instructions' && styles.tabActive]}
          onPress={() => setActiveTab('instructions')}
        >
          <Text style={[
            styles.tabText,
            activeTab === 'instructions' && styles.tabTextActive
          ]}>
            Como Fazer
          </Text>
        </TouchableOpacity>
      </View>

      {/* Tab Content */}
      {activeTab === 'overview' ? (
        <>
          {/* Training Parameters */}
          <View style={styles.parametersSection}>
            <Text style={styles.sectionTitle}>Parâmetros do Treino</Text>

            <View style={styles.parametersGrid}>
              <View style={styles.paramCard}>
                <Icon name="repeat" size={28} color="#007AFF" />
                <Text style={styles.paramValue}>{exercise.sets}</Text>
                <Text style={styles.paramLabel}>Séries</Text>
              </View>

              <View style={styles.paramCard}>
                <Icon name="fitness-outline" size={28} color="#007AFF" />
                <Text style={styles.paramValue}>{exercise.repetitions}</Text>
                <Text style={styles.paramLabel}>Repetições</Text>
              </View>

              {exercise.weight && (
                <View style={styles.paramCard}>
                  <Icon name="barbell-outline" size={28} color="#007AFF" />
                  <Text style={styles.paramValue}>{exercise.weight}kg</Text>
                  <Text style={styles.paramLabel}>Peso</Text>
                </View>
              )}

              <View style={styles.paramCard}>
                <Icon name="time-outline" size={28} color="#007AFF" />
                <Text style={styles.paramValue}>{exercise.restSeconds}s</Text>
                <Text style={styles.paramLabel}>Descanso</Text>
              </View>
            </View>
          </View>

          {/* Equipment Badge */}
          <View style={styles.equipmentSection}>
            <Text style={styles.sectionTitle}>Equipamento</Text>
            <View style={styles.equipmentBadge}>
              <Icon name="construct-outline" size={20} color="#666" />
              <Text style={styles.equipmentText}>{exercise.equipment}</Text>
            </View>
          </View>

          {/* Instructor Notes */}
          {exercise.notes && (
            <View style={styles.notesSection}>
              <View style={styles.notesHeader}>
                <Icon name="information-circle" size={24} color="#007AFF" />
                <Text style={styles.notesTitleText}>
                  Observações do Instrutor
                </Text>
              </View>
              <Text style={styles.notesText}>{exercise.notes}</Text>
            </View>
          )}
        </>
      ) : (
        /* ===== VISUAL CONTENT: Step-by-Step Instructions ===== */
        <View style={styles.instructionsSection}>
          {fullDetails?.instructions ? (
            <>
              <Text style={styles.sectionTitle}>Passo a Passo</Text>
              {fullDetails.instructions.map((instruction, index) => (
                <View key={index} style={styles.instructionStep}>
                  <View style={styles.stepNumber}>
                    <Text style={styles.stepNumberText}>{index + 1}</Text>
                  </View>
                  <Text style={styles.instructionText}>{instruction}</Text>
                </View>
              ))}
            </>
          ) : (
            <View style={styles.emptyInstructions}>
              <Icon name="document-text-outline" size={48} color="#ccc" />
              <Text style={styles.emptyText}>
                Instruções não disponíveis para este exercício
              </Text>
            </View>
          )}
        </View>
      )}
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },

  // ===== GIF ANIMATION STYLES =====
  gifContainer: {
    backgroundColor: '#fff',
    padding: 16,
    alignItems: 'center',
  },
  exerciseGif: {
    width: width - 32,
    height: 300,
    borderRadius: 12,
    backgroundColor: '#f0f0f0',
  },

  headerSection: {
    backgroundColor: '#fff',
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0',
  },
  exerciseName: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#333',
    textAlign: 'center',
  },

  // ===== MUSCLE GROUPS STYLES =====
  muscleSection: {
    backgroundColor: '#fff',
    padding: 16,
    marginTop: 8,
  },
  muscleCard: {
    gap: 16,
  },
  musclePrimary: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#e3f2fd',
    padding: 12,
    borderRadius: 8,
    gap: 12,
  },
  muscleSecondary: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f5f5f5',
    padding: 12,
    borderRadius: 8,
    gap: 12,
  },
  muscleInfo: {
    flex: 1,
  },
  muscleLabel: {
    fontSize: 12,
    color: '#666',
    marginBottom: 4,
  },
  muscleValue: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
    textTransform: 'capitalize',
  },

  tabContainer: {
    flexDirection: 'row',
    backgroundColor: '#fff',
    marginTop: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0',
  },
  tab: {
    flex: 1,
    paddingVertical: 16,
    alignItems: 'center',
    borderBottomWidth: 2,
    borderBottomColor: 'transparent',
  },
  tabActive: {
    borderBottomColor: '#007AFF',
  },
  tabText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#666',
  },
  tabTextActive: {
    color: '#007AFF',
  },

  parametersSection: {
    backgroundColor: '#fff',
    padding: 16,
    marginTop: 8,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
    marginBottom: 16,
  },
  parametersGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 12,
  },
  paramCard: {
    flex: 1,
    minWidth: '45%',
    backgroundColor: '#f8f8f8',
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
  },
  paramValue: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#333',
    marginTop: 8,
  },
  paramLabel: {
    fontSize: 12,
    color: '#666',
    marginTop: 4,
  },

  equipmentSection: {
    backgroundColor: '#fff',
    padding: 16,
    marginTop: 8,
  },
  equipmentBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f5f5f5',
    padding: 12,
    borderRadius: 8,
    gap: 8,
  },
  equipmentText: {
    fontSize: 16,
    color: '#333',
    textTransform: 'capitalize',
  },

  notesSection: {
    backgroundColor: '#fff',
    padding: 16,
    marginTop: 8,
  },
  notesHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginBottom: 12,
  },
  notesTitleText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#007AFF',
  },
  notesText: {
    fontSize: 14,
    color: '#333',
    lineHeight: 22,
    backgroundColor: '#f0f7ff',
    padding: 12,
    borderRadius: 8,
    borderLeftWidth: 4,
    borderLeftColor: '#007AFF',
  },

  // ===== INSTRUCTIONS STYLES =====
  instructionsSection: {
    backgroundColor: '#fff',
    padding: 16,
    marginTop: 8,
  },
  instructionStep: {
    flexDirection: 'row',
    marginBottom: 16,
    gap: 12,
  },
  stepNumber: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: '#007AFF',
    justifyContent: 'center',
    alignItems: 'center',
  },
  stepNumberText: {
    color: '#fff',
    fontWeight: 'bold',
    fontSize: 16,
  },
  instructionText: {
    flex: 1,
    fontSize: 14,
    color: '#333',
    lineHeight: 22,
  },
  emptyInstructions: {
    padding: 40,
    alignItems: 'center',
  },
  emptyText: {
    fontSize: 16,
    color: '#999',
    textAlign: 'center',
    marginTop: 16,
  },
});

export default ExerciseDetailScreen;
```

---

### Visual Content Best Practices

#### 1. GIF Loading Optimization

```typescript
// Add placeholder while GIF loads
<Image
  source={{ uri: exercise.gifUrl }}
  style={styles.exerciseGif}
  resizeMode="contain"
  defaultSource={require('./assets/exercise-placeholder.png')}
  onLoadStart={() => setImageLoading(true)}
  onLoadEnd={() => setImageLoading(false)}
  onError={() => setImageError(true)}
/>

// Show loading indicator
{imageLoading && (
  <View style={styles.imageLoadingOverlay}>
    <ActivityIndicator size="large" color="#007AFF" />
  </View>
)}

// Show error fallback
{imageError && (
  <View style={styles.imageError}>
    <Icon name="image-outline" size={48} color="#ccc" />
    <Text style={styles.errorText}>Imagem não disponível</Text>
  </View>
)}
```

#### 2. Caching GIF Images

```typescript
// Using react-native-fast-image for better performance
import FastImage from 'react-native-fast-image';

<FastImage
  source={{
    uri: exercise.gifUrl,
    priority: FastImage.priority.high,
    cache: FastImage.cacheControl.immutable,
  }}
  style={styles.exerciseGif}
  resizeMode={FastImage.resizeMode.contain}
/>
```

#### 3. Progressive Enhancement

```typescript
// Show basic info first, load detailed info in background
useEffect(() => {
  // Immediate: Show basic exercise data from workout
  setExercise(basicExerciseData);
  setLoading(false);

  // Background: Fetch full details from ExerciseDB
  fetchFullDetails(exerciseDbId).then(details => {
    setFullDetails(details);
  });
}, []);
```

---

### Visual Content Summary

| Content Type | Source | Display Location | Purpose |
|--------------|--------|------------------|---------|
| **GIF Animation** | `exercise.gifUrl` | Top of screen | Show exercise movement |
| **Muscle Groups** | `fullDetails.target`, `secondaryMuscles` | Below GIF | Show targeted muscles |
| **Equipment Badge** | `exercise.equipment` | Overview tab | Show required equipment |
| **Training Parameters** | `sets`, `reps`, `weight`, `rest` | Overview tab | Show workout specifics |
| **Instructor Notes** | `exercise.notes` | Overview tab | Show custom guidance |
| **Step Instructions** | `fullDetails.instructions` | Instructions tab | Show execution steps |

---

### Testing Visual Content

```bash
# Test exercise details endpoint
curl -X GET "http://localhost:7009/api/exercises/0001" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Verify GIF URL is accessible
curl -I "https://exercisedb.p.rapidapi.com/exercises/0001.gif"
```

---

## Summary

### Dashboard Endpoint
✅ **Implemented:** `GET /api/students/dashboard`
✅ **Returns:** Aggregated student data in single call
✅ **Features:** Trainer info, workout count, active workout ID

### Visual Content
✅ **Available:** GIF animations, muscle groups, instructions
✅ **Source:** Exercise entity + ExerciseDB API
✅ **Display:** Optimized React Native components

Both features are production-ready and can be integrated into your React Native app immediately!
