# Coach-Student Connection Guide

Complete guide for implementing the coach-student connection feature in your React Native/Expo frontend.

---

## Table of Contents

1. [Overview](#overview)
2. [How It Works](#how-it-works)
3. [Backend API Reference](#backend-api-reference)
4. [Frontend Implementation](#frontend-implementation)
5. [Complete Code Examples](#complete-code-examples)
6. [Testing Guide](#testing-guide)
7. [Common Issues & Solutions](#common-issues--solutions)

---

## Overview

### What This Feature Does

Before a coach can create workout plans for a student, they need to establish a connection. This is the **simplest possible implementation**:

1. **Coach** enters the **student's email address**
2. **System** finds the student and creates the connection
3. **Coach** can now create workouts for that student

### Key Benefits

- ✅ **Simple**: Just needs an email address
- ✅ **No invite codes** to manage
- ✅ **No approval flow** - instant connection
- ✅ **No emails sent** - purely in-app
- ✅ **Idempotent** - connecting twice is safe

---

## How It Works

### User Flow Diagram

```
COACH SIDE:
┌─────────────────────────────────────┐
│  Coach opens "Add Student" screen   │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  Coach enters student's email       │
│  Example: "john@example.com"        │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  POST /api/instructors/connect      │
│  { "email": "john@example.com" }    │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  ✅ Connection Created!             │
│  Student appears in coach's list    │
└─────────────────────────────────────┘

STUDENT SIDE:
┌─────────────────────────────────────┐
│  Student can see their coach        │
│  GET /api/students/current-trainer  │
└─────────────────────────────────────┘
```

### Prerequisites

1. **Student must have an account** - They need to register first
2. **Coach must be logged in** - Need JWT token
3. **Student must be of type "Student"** - Can't connect to other coaches

---

## Backend API Reference

### Base URL

```
http://YOUR_SERVER_IP:7009/api
```

Replace `YOUR_SERVER_IP` with your actual server IP (not `localhost` if testing on mobile).

---

### 1. Connect Coach to Student

**Endpoint**: `POST /api/instructors/connect`

**Authentication**: Required (Coach JWT token)

**Request Headers**:
```http
Authorization: Bearer {coach_jwt_token}
Content-Type: application/json
```

**Request Body** (Option 1 - By Email):
```json
{
  "email": "student@example.com"
}
```

**Request Body** (Option 2 - By Student ID):
```json
{
  "studentId": 123
}
```

**Success Response** (200 OK):
```json
{
  "message": "Conectado com sucesso ao aluno."
}
```

**Error Responses**:

| Status | Reason | Response |
|--------|--------|----------|
| 400 | No email or studentId provided | `{ "message": "Não foi possível conectar com o aluno." }` |
| 400 | User is not a student | `{ "message": "Não foi possível conectar com o aluno." }` |
| 401 | Not authenticated | `{ "message": "Usuário não autenticado." }` |
| 403 | User is not a coach | `Forbid` |
| 404 | Student not found | `{ "message": "Não foi possível conectar com o aluno." }` |

---

### 2. Get Coach's Students List

**Endpoint**: `GET /api/instructors/students`

**Authentication**: Required (Coach JWT token)

**Success Response** (200 OK):
```json
[
  {
    "id": 5002,
    "name": "John Doe",
    "email": "john@example.com", 
    "age": 25,
    "activeWorkoutId": 101,
    "activeWorkoutName": "Hipertrofia Fase 1",
    "completedExercisesToday": 3,
    "totalExercisesToday": 8
  },
  {
    "id": 5003,
    "name": "Jane Smith",
    "email": "jane@example.com",
    "age": 30,
    "activeWorkoutId": 0,
    "activeWorkoutName": "Nenhum treino ativo",
    "completedExercisesToday": 0,
    "totalExercisesToday": 0
  }
]
```

---

### 3. Get Student Details

**Endpoint**: `GET /api/instructors/students/{studentId}`

**Authentication**: Required (Coach JWT token)

**URL Parameters**:
- `studentId` (integer) - ID of the student

**Success Response** (200 OK):
```json
{
  "id": 5002,
  "name": "John Doe",
  "email": "john@example.com",
  "age": 25,
  "activeWorkoutId": 101,
  "activeWorkoutName": "Hipertrofia Fase 1",
  "completedExercisesToday": 3,
  "totalExercisesToday": 8
}
```

**Error Responses**:

| Status | Reason | Response |
|--------|--------|----------|
| 404 | Student not found or not connected | `{ "message": "Aluno não encontrado ou não está vinculado a este instrutor." }` |

---

### 4. Remove Student Connection

**Endpoint**: `DELETE /api/instructors/students/{studentId}`

**Authentication**: Required (Coach JWT token)

**URL Parameters**:
- `studentId` (integer) - ID of the student to disconnect

**Success Response** (200 OK):
```json
{
  "message": "Aluno removido com sucesso."
}
```

**Error Responses**:

| Status | Reason | Response |
|--------|--------|----------|
| 404 | Student not found or not connected | `{ "message": "Aluno não encontrado ou não está vinculado a este instrutor." }` |

**Important**: This only removes the connection. All existing workout plans for the student are **preserved**.

---

### 5. Get Student's Current Trainer (Student Endpoint)

**Endpoint**: `GET /api/students/current-trainer`

**Authentication**: Required (Student JWT token)

**Success Response** (200 OK) - Has Trainer:
```json
{
  "hasTrainer": true,
  "trainer": {
    "id": 1,
    "name": "Coach Mike",
    "email": "mike@coach.com",
    "description": "Certified Personal Trainer"
  }
}
```

**Success Response** (200 OK) - No Trainer:
```json
{
  "hasTrainer": false,
  "message": "No trainer assigned",
  "trainer": null
}
```

**Note**: A student can have **multiple trainers**. This endpoint returns the first one found.

---

## Frontend Implementation

### Step 1: Create TypeScript Types

Create a file: `src/types/instructor.types.ts`

```typescript
/**
 * Request to connect with a student
 */
export interface ConnectStudentRequest {
  email?: string;       // Student's email (option 1)
  studentId?: number;   // Student's ID (option 2)
}

/**
 * Student summary information for coach's view
 */
export interface StudentSummary {
  id: number;
  name: string;
  email: string;
  age: number | null;
  activeWorkoutId: number;
  activeWorkoutName: string;
  completedExercisesToday: number;
  totalExercisesToday: number;
}

/**
 * Instructor/Coach information
 */
export interface InstructorInfo {
  id: number;
  name: string;
  email: string;
  description: string;
}

/**
 * Response when checking student's trainer
 */
export interface StudentTrainerResponse {
  hasTrainer: boolean;
  trainer: InstructorInfo | null;
  message?: string;
}
```

---

### Step 2: Create API Service

Create a file: `src/services/instructor.service.ts`

```typescript
import apiClient from '../config/api'; // Your axios instance
import {
  ConnectStudentRequest,
  StudentSummary,
  InstructorInfo,
} from '../types/instructor.types';

export const instructorService = {
  /**
   * Connect with a student by email or ID
   * @param data - Student email or ID
   * @throws Error if connection fails
   */
  connectWithStudent: async (data: ConnectStudentRequest): Promise<void> => {
    const response = await apiClient.post('/instructors/connect', data);
    return response.data;
  },

  /**
   * Get all students connected to the current coach
   * @returns Array of students with their workout summaries
   */
  getMyStudents: async (): Promise<StudentSummary[]> => {
    const response = await apiClient.get('/instructors/students');
    return response.data;
  },

  /**
   * Get detailed information for a specific student
   * @param studentId - ID of the student
   * @returns Student details
   * @throws Error if student not found or not connected
   */
  getStudentDetails: async (studentId: number): Promise<StudentSummary> => {
    const response = await apiClient.get(`/instructors/students/${studentId}`);
    return response.data;
  },

  /**
   * Remove connection with a student
   * Note: This does NOT delete the student's workouts
   * @param studentId - ID of the student
   * @throws Error if student not found or not connected
   */
  removeStudent: async (studentId: number): Promise<void> => {
    const response = await apiClient.delete(`/instructors/students/${studentId}`);
    return response.data;
  },
};
```

Create a file: `src/services/student.service.ts`

```typescript
import apiClient from '../config/api';
import { StudentTrainerResponse } from '../types/instructor.types';

export const studentService = {
  /**
   * Get the current student's assigned coach/trainer
   * @returns Trainer information or null if no trainer
   */
  getCurrentTrainer: async (): Promise<StudentTrainerResponse> => {
    const response = await apiClient.get('/students/current-trainer');
    return response.data;
  },
};
```

---

## Complete Code Examples

### Example 1: Add Student Screen (Coach)

This is the main screen where a coach adds a new student by email.

**File**: `src/screens/coach/AddStudentScreen.tsx`

```typescript
import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  Alert,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
} from 'react-native';
import { instructorService } from '../../services/instructor.service';

interface AddStudentScreenProps {
  navigation: any; // Replace with proper navigation type
}

export const AddStudentScreen: React.FC<AddStudentScreenProps> = ({ navigation }) => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleConnect = async () => {
    // Validation
    if (!email.trim()) {
      Alert.alert('Campo obrigatório', 'Por favor, digite o email do aluno.');
      return;
    }

    if (!validateEmail(email.trim())) {
      Alert.alert('Email inválido', 'Por favor, digite um email válido.');
      return;
    }

    try {
      setLoading(true);

      // Call API
      await instructorService.connectWithStudent({
        email: email.trim().toLowerCase(),
      });

      // Success
      Alert.alert(
        '✅ Aluno conectado!',
        'Agora você pode criar treinos para este aluno.',
        [
          {
            text: 'OK',
            onPress: () => {
              navigation.goBack(); // Go back to students list
            },
          },
        ]
      );
    } catch (error: any) {
      console.error('Error connecting with student:', error);

      // Handle specific error cases
      if (error.response) {
        const status = error.response.status;

        switch (status) {
          case 400:
            Alert.alert(
              'Erro',
              'O usuário não é um aluno ou já está conectado com você.'
            );
            break;
          case 404:
            Alert.alert(
              'Aluno não encontrado',
              'Nenhum aluno encontrado com este email.\n\nVerifique se:\n• O email está correto\n• O aluno já criou uma conta no app'
            );
            break;
          case 401:
            Alert.alert('Sessão expirada', 'Faça login novamente.');
            // Navigate to login
            break;
          default:
            Alert.alert('Erro', 'Não foi possível conectar com o aluno.');
        }
      } else {
        Alert.alert(
          'Erro de conexão',
          'Verifique sua internet e tente novamente.'
        );
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <View style={styles.content}>
        {/* Header */}
        <Text style={styles.title}>Adicionar Aluno</Text>
        <Text style={styles.subtitle}>
          Digite o email que o aluno usou para criar a conta no aplicativo.
        </Text>

        {/* Email Input */}
        <View style={styles.inputContainer}>
          <Text style={styles.label}>Email do Aluno</Text>
          <TextInput
            style={styles.input}
            placeholder="exemplo@email.com"
            placeholderTextColor="#999"
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            autoCorrect={false}
            autoComplete="email"
            editable={!loading}
            returnKeyType="done"
            onSubmitEditing={handleConnect}
          />
        </View>

        {/* Connect Button */}
        <TouchableOpacity
          style={[styles.button, loading && styles.buttonDisabled]}
          onPress={handleConnect}
          disabled={loading}
          activeOpacity={0.8}
        >
          {loading ? (
            <ActivityIndicator color="#fff" />
          ) : (
            <Text style={styles.buttonText}>Conectar Aluno</Text>
          )}
        </TouchableOpacity>

        {/* Info Box */}
        <View style={styles.infoBox}>
          <Text style={styles.infoIcon}>💡</Text>
          <View style={styles.infoTextContainer}>
            <Text style={styles.infoTitle}>Dica</Text>
            <Text style={styles.infoText}>
              Peça ao seu aluno para compartilhar o email que ele usou no cadastro.
              O aluno precisa ter criado uma conta antes de você poder conectá-lo.
            </Text>
          </View>
        </View>
      </View>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  content: {
    flex: 1,
    padding: 20,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#000',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 15,
    color: '#666',
    marginBottom: 32,
    lineHeight: 22,
  },
  inputContainer: {
    marginBottom: 24,
  },
  label: {
    fontSize: 16,
    fontWeight: '600',
    color: '#000',
    marginBottom: 8,
  },
  input: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 12,
    padding: 16,
    fontSize: 16,
    color: '#000',
    backgroundColor: '#f9f9f9',
  },
  button: {
    backgroundColor: '#007AFF',
    padding: 18,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 56,
  },
  buttonDisabled: {
    opacity: 0.6,
  },
  buttonText: {
    color: '#fff',
    fontSize: 17,
    fontWeight: '600',
  },
  infoBox: {
    marginTop: 32,
    padding: 16,
    backgroundColor: '#f0f9ff',
    borderRadius: 12,
    borderLeftWidth: 4,
    borderLeftColor: '#007AFF',
    flexDirection: 'row',
  },
  infoIcon: {
    fontSize: 24,
    marginRight: 12,
  },
  infoTextContainer: {
    flex: 1,
  },
  infoTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#000',
    marginBottom: 6,
  },
  infoText: {
    fontSize: 14,
    color: '#666',
    lineHeight: 20,
  },
});
```

---

### Example 2: Students List Screen (Coach)

Display all connected students with their workout progress.

**File**: `src/screens/coach/StudentsListScreen.tsx`

```typescript
import React, { useEffect, useState, useCallback } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  Alert,
  StyleSheet,
  RefreshControl,
  ActivityIndicator,
} from 'react-native';
import { instructorService } from '../../services/instructor.service';
import { StudentSummary } from '../../types/instructor.types';

interface StudentsListScreenProps {
  navigation: any;
}

export const StudentsListScreen: React.FC<StudentsListScreenProps> = ({ navigation }) => {
  const [students, setStudents] = useState<StudentSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  // Load students on mount
  useEffect(() => {
    loadStudents();
  }, []);

  // Reload when screen comes into focus
  useEffect(() => {
    const unsubscribe = navigation.addListener('focus', () => {
      loadStudents();
    });
    return unsubscribe;
  }, [navigation]);

  const loadStudents = async () => {
    try {
      setLoading(true);
      const data = await instructorService.getMyStudents();
      setStudents(data);
    } catch (error) {
      console.error('Error loading students:', error);
      Alert.alert('Erro', 'Não foi possível carregar a lista de alunos.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadStudents();
  }, []);

  const handleAddStudent = () => {
    navigation.navigate('AddStudent');
  };

  const handleStudentPress = (student: StudentSummary) => {
    navigation.navigate('StudentWorkouts', {
      studentId: student.id,
      studentName: student.name,
    });
  };

  const handleRemoveStudent = (student: StudentSummary) => {
    Alert.alert(
      'Remover Aluno',
      `Tem certeza que deseja remover ${student.name}?\n\nOs treinos criados para este aluno serão mantidos.`,
      [
        {
          text: 'Cancelar',
          style: 'cancel',
        },
        {
          text: 'Remover',
          style: 'destructive',
          onPress: async () => {
            try {
              await instructorService.removeStudent(student.id);
              Alert.alert('Sucesso', `${student.name} foi removido da sua lista.`);
              loadStudents(); // Reload list
            } catch (error) {
              console.error('Error removing student:', error);
              Alert.alert('Erro', 'Não foi possível remover o aluno.');
            }
          },
        },
      ]
    );
  };

  const renderStudent = ({ item }: { item: StudentSummary }) => (
    <TouchableOpacity
      style={styles.studentCard}
      onPress={() => handleStudentPress(item)}
      activeOpacity={0.7}
    >
      {/* Student Info */}
      <View style={styles.studentInfo}>
        <Text style={styles.studentName}>{item.name}</Text>
        <Text style={styles.studentEmail}>{item.email}</Text>

        {/* Current Workout */}
        <View style={styles.workoutBadge}>
          <Text style={styles.workoutBadgeText}>
            {item.activeWorkoutId > 0 ? '🏋️ ' + item.activeWorkoutName : '⚠️ Sem treino ativo'}
          </Text>
        </View>

        {/* Today's Progress */}
        {item.totalExercisesToday > 0 && (
          <View style={styles.progressContainer}>
            <Text style={styles.progressText}>
              Progresso de hoje: {item.completedExercisesToday}/{item.totalExercisesToday} exercícios
            </Text>
            <View style={styles.progressBar}>
              <View
                style={[
                  styles.progressFill,
                  {
                    width: `${(item.completedExercisesToday / item.totalExercisesToday) * 100}%`,
                  },
                ]}
              />
            </View>
          </View>
        )}
      </View>

      {/* Actions */}
      <View style={styles.actionsContainer}>
        <TouchableOpacity
          style={styles.manageButton}
          onPress={() => handleStudentPress(item)}
        >
          <Text style={styles.manageButtonText}>Gerenciar</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={styles.removeButton}
          onPress={() => handleRemoveStudent(item)}
        >
          <Text style={styles.removeButtonText}>Remover</Text>
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );

  const renderEmptyState = () => (
    <View style={styles.emptyState}>
      <Text style={styles.emptyIcon}>👥</Text>
      <Text style={styles.emptyTitle}>Nenhum aluno conectado</Text>
      <Text style={styles.emptyText}>
        Adicione seu primeiro aluno para começar a criar treinos personalizados.
      </Text>
      <TouchableOpacity style={styles.emptyButton} onPress={handleAddStudent}>
        <Text style={styles.emptyButtonText}>+ Adicionar Aluno</Text>
      </TouchableOpacity>
    </View>
  );

  if (loading && !refreshing) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#007AFF" />
        <Text style={styles.loadingText}>Carregando alunos...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <FlatList
        data={students}
        renderItem={renderStudent}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={students.length === 0 ? styles.emptyList : styles.list}
        ListEmptyComponent={renderEmptyState}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={onRefresh}
            tintColor="#007AFF"
          />
        }
      />

      {/* Floating Add Button */}
      {students.length > 0 && (
        <TouchableOpacity style={styles.fab} onPress={handleAddStudent}>
          <Text style={styles.fabText}>+</Text>
        </TouchableOpacity>
      )}
    </View>
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
    backgroundColor: '#f5f5f5',
  },
  loadingText: {
    marginTop: 12,
    fontSize: 16,
    color: '#666',
  },
  list: {
    padding: 16,
  },
  emptyList: {
    flexGrow: 1,
  },
  studentCard: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  studentInfo: {
    marginBottom: 12,
  },
  studentName: {
    fontSize: 18,
    fontWeight: '600',
    color: '#000',
    marginBottom: 4,
  },
  studentEmail: {
    fontSize: 14,
    color: '#666',
    marginBottom: 8,
  },
  workoutBadge: {
    alignSelf: 'flex-start',
    backgroundColor: '#f0f9ff',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 16,
    marginBottom: 8,
  },
  workoutBadgeText: {
    fontSize: 13,
    color: '#007AFF',
    fontWeight: '500',
  },
  progressContainer: {
    marginTop: 8,
  },
  progressText: {
    fontSize: 13,
    color: '#666',
    marginBottom: 6,
  },
  progressBar: {
    height: 6,
    backgroundColor: '#e0e0e0',
    borderRadius: 3,
    overflow: 'hidden',
  },
  progressFill: {
    height: '100%',
    backgroundColor: '#4CAF50',
    borderRadius: 3,
  },
  actionsContainer: {
    flexDirection: 'row',
    gap: 8,
  },
  manageButton: {
    flex: 1,
    backgroundColor: '#007AFF',
    paddingVertical: 10,
    borderRadius: 8,
    alignItems: 'center',
  },
  manageButtonText: {
    color: '#fff',
    fontSize: 15,
    fontWeight: '600',
  },
  removeButton: {
    flex: 1,
    backgroundColor: '#fff',
    paddingVertical: 10,
    borderRadius: 8,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: '#FF3B30',
  },
  removeButtonText: {
    color: '#FF3B30',
    fontSize: 15,
    fontWeight: '600',
  },
  emptyState: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 40,
  },
  emptyIcon: {
    fontSize: 64,
    marginBottom: 16,
  },
  emptyTitle: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#000',
    marginBottom: 8,
  },
  emptyText: {
    fontSize: 15,
    color: '#666',
    textAlign: 'center',
    lineHeight: 22,
    marginBottom: 24,
  },
  emptyButton: {
    backgroundColor: '#007AFF',
    paddingHorizontal: 24,
    paddingVertical: 14,
    borderRadius: 12,
  },
  emptyButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  fab: {
    position: 'absolute',
    right: 20,
    bottom: 20,
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: '#007AFF',
    justifyContent: 'center',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 6,
    elevation: 8,
  },
  fabText: {
    color: '#fff',
    fontSize: 32,
    fontWeight: '300',
  },
});
```

---

### Example 3: View Coach Screen (Student)

Allow students to see who their trainer is.

**File**: `src/screens/student/ViewCoachScreen.tsx`

```typescript
import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Linking,
} from 'react-native';
import { studentService } from '../../services/student.service';
import { InstructorInfo } from '../../types/instructor.types';

export const ViewCoachScreen: React.FC = () => {
  const [coach, setCoach] = useState<InstructorInfo | null>(null);
  const [hasCoach, setHasCoach] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadCoach();
  }, []);

  const loadCoach = async () => {
    try {
      setLoading(true);
      const data = await studentService.getCurrentTrainer();
      setHasCoach(data.hasTrainer);
      setCoach(data.trainer);
    } catch (error) {
      console.error('Error loading coach:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleEmailPress = () => {
    if (coach?.email) {
      Linking.openURL(`mailto:${coach.email}`);
    }
  };

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#007AFF" />
        <Text style={styles.loadingText}>Carregando...</Text>
      </View>
    );
  }

  if (!hasCoach || !coach) {
    return (
      <View style={styles.emptyContainer}>
        <Text style={styles.emptyIcon}>🏋️</Text>
        <Text style={styles.emptyTitle}>Nenhum treinador</Text>
        <Text style={styles.emptyText}>
          Você ainda não tem um treinador atribuído.{'\n\n'}
          Entre em contato com seu treinador e peça para ele adicionar você usando seu email.
        </Text>
        <View style={styles.infoBox}>
          <Text style={styles.infoText}>
            💡 Seu email cadastrado:{'\n'}
            <Text style={styles.infoEmail}>
              {/* You'll need to get user email from context/state */}
              (ver nas configurações)
            </Text>
          </Text>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.card}>
        {/* Header */}
        <View style={styles.header}>
          <View style={styles.avatarContainer}>
            <Text style={styles.avatarText}>
              {coach.name.charAt(0).toUpperCase()}
            </Text>
          </View>
          <View style={styles.headerInfo}>
            <Text style={styles.label}>Meu Treinador</Text>
            <Text style={styles.coachName}>{coach.name}</Text>
          </View>
        </View>

        {/* Email */}
        <TouchableOpacity style={styles.infoRow} onPress={handleEmailPress}>
          <Text style={styles.infoLabel}>📧 Email</Text>
          <Text style={styles.infoValue}>{coach.email}</Text>
        </TouchableOpacity>

        {/* Description */}
        {coach.description && (
          <View style={styles.descriptionContainer}>
            <Text style={styles.descriptionLabel}>Sobre</Text>
            <Text style={styles.descriptionText}>{coach.description}</Text>
          </View>
        )}

        {/* Contact Button */}
        <TouchableOpacity style={styles.contactButton} onPress={handleEmailPress}>
          <Text style={styles.contactButtonText}>Enviar Email</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
    padding: 16,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#f5f5f5',
  },
  loadingText: {
    marginTop: 12,
    fontSize: 16,
    color: '#666',
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 32,
    backgroundColor: '#f5f5f5',
  },
  emptyIcon: {
    fontSize: 64,
    marginBottom: 16,
  },
  emptyTitle: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#000',
    marginBottom: 12,
  },
  emptyText: {
    fontSize: 15,
    color: '#666',
    textAlign: 'center',
    lineHeight: 22,
    marginBottom: 24,
  },
  infoBox: {
    backgroundColor: '#f0f9ff',
    padding: 16,
    borderRadius: 12,
    borderLeftWidth: 4,
    borderLeftColor: '#007AFF',
  },
  infoText: {
    fontSize: 14,
    color: '#666',
    lineHeight: 20,
  },
  infoEmail: {
    fontWeight: '600',
    color: '#000',
  },
  card: {
    backgroundColor: '#fff',
    borderRadius: 16,
    padding: 20,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 8,
    elevation: 4,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 24,
    paddingBottom: 20,
    borderBottomWidth: 1,
    borderBottomColor: '#f0f0f0',
  },
  avatarContainer: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: '#007AFF',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 16,
  },
  avatarText: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#fff',
  },
  headerInfo: {
    flex: 1,
  },
  label: {
    fontSize: 13,
    color: '#999',
    marginBottom: 4,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  coachName: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#000',
  },
  infoRow: {
    paddingVertical: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#f0f0f0',
  },
  infoLabel: {
    fontSize: 14,
    color: '#666',
    marginBottom: 6,
  },
  infoValue: {
    fontSize: 16,
    color: '#007AFF',
    fontWeight: '500',
  },
  descriptionContainer: {
    marginTop: 20,
    padding: 16,
    backgroundColor: '#f9f9f9',
    borderRadius: 12,
  },
  descriptionLabel: {
    fontSize: 14,
    fontWeight: '600',
    color: '#000',
    marginBottom: 8,
  },
  descriptionText: {
    fontSize: 15,
    color: '#666',
    lineHeight: 22,
  },
  contactButton: {
    marginTop: 24,
    backgroundColor: '#007AFF',
    paddingVertical: 16,
    borderRadius: 12,
    alignItems: 'center',
  },
  contactButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});
```

---

## Testing Guide

### Quick Test with cURL/Postman

**Step 1: Register test users**

```bash
# Register a coach
curl -X POST http://localhost:7009/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Coach Mike",
    "email": "coach@test.com",
    "password": "Test123!",
    "userType": 1
  }'

# Register a student
curl -X POST http://localhost:7009/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Student",
    "email": "student@test.com",
    "password": "Test123!",
    "userType": 2
  }'
```

**Step 2: Login as coach**

```bash
curl -X POST http://localhost:7009/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "coach@test.com",
    "password": "Test123!"
  }'

# Save the "accessToken" from response
```

**Step 3: Connect to student**

```bash
curl -X POST http://localhost:7009/api/instructors/connect \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "email": "student@test.com"
  }'

# Expected response:
# { "message": "Conectado com sucesso ao aluno." }
```

**Step 4: Verify connection**

```bash
# Get coach's students list
curl -X GET http://localhost:7009/api/instructors/students \
  -H "Authorization: Bearer YOUR_COACH_TOKEN"

# Should return array with the student
```

**Step 5: Test from student side**

```bash
# Login as student
curl -X POST http://localhost:7009/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "student@test.com",
    "password": "Test123!"
  }'

# Get current trainer
curl -X GET http://localhost:7009/api/students/current-trainer \
  -H "Authorization: Bearer YOUR_STUDENT_TOKEN"

# Should return the coach info
```

---

### Frontend Testing Checklist

#### Coach Flow

- [ ] Can open "Add Student" screen
- [ ] Email validation works (shows error for invalid emails)
- [ ] Can successfully connect with existing student
- [ ] Shows error when student email doesn't exist
- [ ] Shows error when trying to connect to another coach
- [ ] Successfully connected student appears in students list
- [ ] Can see student's active workout name
- [ ] Can see student's progress for today
- [ ] Can navigate to student's workouts
- [ ] Can remove student connection
- [ ] Confirmation dialog appears before removing
- [ ] List refreshes after removing student
- [ ] Pull-to-refresh works on students list
- [ ] Empty state shows when no students
- [ ] Loading states display correctly

#### Student Flow

- [ ] Can see assigned coach information
- [ ] Shows empty state when no coach assigned
- [ ] Can tap to send email to coach
- [ ] Coach's description displays correctly (if present)

---

## Common Issues & Solutions

### Issue 1: "Student not found" error

**Symptoms**: Getting 404 or "Aluno não encontrado" even though student exists

**Solutions**:
1. **Verify email is correct** - Check for typos, extra spaces
2. **Confirm student account exists** - Student must register first
3. **Check email case** - Try lowercase: `email.trim().toLowerCase()`
4. **Verify user type** - User must be a Student (userType: 2), not Instructor

**Debug code**:
```typescript
console.log('Searching for email:', email.trim().toLowerCase());
```

---

### Issue 2: "Already connected" but student not in list

**Symptoms**: Connection succeeds but student doesn't appear in coach's list

**Solutions**:
1. **Refresh the list** - Pull to refresh or reload screen
2. **Check network** - Verify response data is being received
3. **Check filters** - Ensure you're not filtering out the student

**Debug code**:
```typescript
const data = await instructorService.getMyStudents();
console.log('Students received:', data.length);
console.log('Student IDs:', data.map(s => s.id));
```

---

### Issue 3: 401 Unauthorized

**Symptoms**: All requests return 401 even with valid token

**Solutions**:
1. **Check token format** - Must be `Bearer YOUR_TOKEN`
2. **Verify token not expired** - Tokens expire after 1 hour
3. **Check axios interceptor** - Ensure it's adding the header correctly

**Debug code**:
```typescript
// In your axios interceptor
apiClient.interceptors.request.use(async (config) => {
  const token = await AsyncStorage.getItem('authToken');
  console.log('Token being sent:', token ? 'YES' : 'NO');
  console.log('Headers:', config.headers);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

---

### Issue 4: Network request failed (Mobile)

**Symptoms**: Requests work on web but fail on mobile device

**Solutions**:
1. **Use actual IP, not localhost**
   ```typescript
   // ❌ Wrong
   const API_URL = 'http://localhost:7009/api';

   // ✅ Correct
   const API_URL = 'http://192.168.1.100:7009/api';
   ```

2. **Find your IP**:
   - Windows: `ipconfig` in Command Prompt
   - Mac/Linux: `ifconfig` or `ip addr`
   - Look for IPv4 address (usually starts with 192.168 or 10.0)

3. **Ensure device is on same WiFi** - Phone and computer must be on same network

4. **Check firewall** - May be blocking connections

---

### Issue 5: Email validation too strict/loose

**Solution**: Use this standard email regex

```typescript
const validateEmail = (email: string): boolean => {
  // Standard email validation
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

// Examples:
validateEmail('test@example.com')      // ✅ true
validateEmail('test.name@example.com') // ✅ true
validateEmail('test@subdomain.example.com') // ✅ true
validateEmail('test@example')          // ❌ false
validateEmail('test.example.com')      // ❌ false
validateEmail('@example.com')          // ❌ false
```

---

## Best Practices

### 1. Always Trim and Lowercase Emails

```typescript
const normalizeEmail = (email: string): string => {
  return email.trim().toLowerCase();
};

// Use it
await instructorService.connectWithStudent({
  email: normalizeEmail(email),
});
```

### 2. Handle Loading States

```typescript
const [loading, setLoading] = useState(false);

const handleConnect = async () => {
  setLoading(true);
  try {
    await instructorService.connectWithStudent({ email });
  } catch (error) {
    // Handle error
  } finally {
    setLoading(false); // Always reset loading
  }
};
```

### 3. Provide Clear Error Messages

```typescript
// ❌ Bad
Alert.alert('Error', 'Failed');

// ✅ Good
Alert.alert(
  'Student Not Found',
  'No student found with this email.\n\nMake sure:\n• The email is correct\n• The student has created an account'
);
```

### 4. Use Pull-to-Refresh

```typescript
<FlatList
  data={students}
  refreshControl={
    <RefreshControl
      refreshing={refreshing}
      onRefresh={loadStudents}
    />
  }
/>
```

### 5. Optimistic Updates (Optional)

For better UX, you can update UI before API confirms:

```typescript
const handleConnect = async (email: string) => {
  try {
    // Add to list immediately (optimistic)
    const tempStudent = {
      id: -1,
      name: 'Carregando...',
      email: email,
      // ... other fields
    };
    setStudents(prev => [...prev, tempStudent]);

    // Make API call
    await instructorService.connectWithStudent({ email });

    // Reload to get real data
    await loadStudents();
  } catch (error) {
    // Remove temp student on error
    setStudents(prev => prev.filter(s => s.id !== -1));
    // Show error
  }
};
```

---

## Summary

### What You've Learned

✅ **Backend API** - All 5 endpoints for connection management
✅ **TypeScript Types** - Complete type definitions
✅ **API Services** - Ready-to-use service functions
✅ **UI Screens** - 3 complete, production-ready screens
✅ **Error Handling** - Proper error messages and edge cases
✅ **Testing** - How to test with cURL and in your app
✅ **Debugging** - Common issues and how to fix them

### Next Steps

1. **Copy the code** into your project
2. **Update your API base URL** in `api.config.ts`
3. **Test with sample data** using cURL commands
4. **Implement in your app** starting with Add Student screen
5. **Test edge cases** (wrong email, no internet, etc.)
6. **Style to match your design** system

### Quick Reference Card

```typescript
// Connect student
await instructorService.connectWithStudent({
  email: "student@example.com"
});

// Get all students
const students = await instructorService.getMyStudents();

// Get student details
const student = await instructorService.getStudentDetails(studentId);

// Remove student
await instructorService.removeStudent(studentId);

// Get student's coach (student endpoint)
const { hasTrainer, trainer } = await studentService.getCurrentTrainer();
```

---

**Need help?** Check the troubleshooting section or review the complete `FRONTEND_INTEGRATION_GUIDE.md` for more details.

**Happy coding! 🚀**
