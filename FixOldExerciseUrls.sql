-- ========================================
-- Fix Old Exercise Image URLs
-- ========================================
-- This script updates exercises that have old v2.exercisedb.io URLs
-- to the new RapidAPI format with resolution parameter
--
-- Date: 2025-10-29
-- Purpose: Fix broken exercise GIF URLs
-- ========================================

-- Backup current data (optional but recommended)
-- SELECT * INTO Exercises_Backup FROM Exercises;

-- Preview exercises that will be updated
SELECT
    Id,
    Name,
    ExerciseDbId,
    GifUrl AS OldUrl,
    CASE
        WHEN GifUrl LIKE '%v2.exercisedb.io%' THEN
            'https://exercisedb.p.rapidapi.com/image?exerciseId=' + ExerciseDbId + '&resolution=720'
        ELSE
            GifUrl
    END AS NewUrl
FROM Exercises
WHERE GifUrl LIKE '%v2.exercisedb.io%';

-- Update exercises with old URLs to new RapidAPI format
-- Default resolution: 720 (student view quality)
UPDATE Exercises
SET GifUrl = 'https://exercisedb.p.rapidapi.com/image?exerciseId=' + ExerciseDbId + '&resolution=720'
WHERE GifUrl LIKE '%v2.exercisedb.io%';

-- Verify the update
SELECT
    COUNT(*) as TotalUpdated
FROM Exercises
WHERE GifUrl LIKE '%exercisedb.p.rapidapi.com%';

-- Show all exercises with their new URLs
SELECT
    Id,
    Name,
    ExerciseDbId,
    BodyPart,
    Equipment,
    GifUrl
FROM Exercises
ORDER BY Id;

-- ========================================
-- Alternative: Update to instructor resolution (360px)
-- Uncomment if you want smaller images for all exercises
-- ========================================
/*
UPDATE Exercises
SET GifUrl = 'https://exercisedb.p.rapidapi.com/image?exerciseId=' + ExerciseDbId + '&resolution=360'
WHERE GifUrl LIKE '%exercisedb.p.rapidapi.com%';
*/

-- ========================================
-- Rollback script (if needed)
-- ========================================
/*
-- Restore from backup
UPDATE Exercises
SET GifUrl = b.GifUrl
FROM Exercises e
INNER JOIN Exercises_Backup b ON e.Id = b.Id;

-- Or manually revert to old format (not recommended - URLs are broken)
UPDATE Exercises
SET GifUrl = 'https://v2.exercisedb.io/image/' + ExerciseDbId
WHERE GifUrl LIKE '%exercisedb.p.rapidapi.com%';
*/

-- ========================================
-- Statistics
-- ========================================
SELECT
    'Total Exercises' as Metric,
    COUNT(*) as Count
FROM Exercises

UNION ALL

SELECT
    'RapidAPI URLs' as Metric,
    COUNT(*) as Count
FROM Exercises
WHERE GifUrl LIKE '%exercisedb.p.rapidapi.com%'

UNION ALL

SELECT
    'Old v2.exercisedb.io URLs' as Metric,
    COUNT(*) as Count
FROM Exercises
WHERE GifUrl LIKE '%v2.exercisedb.io%';
