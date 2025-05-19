# largesort

## Overview

**largesort** is a project designed to handle sorting and generation of extremely large text files (up to 100 GB or more). Each line in the file follows the format:

```
Number. String
```

### Example:
```
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
```

### Sorting Rules:
- Primary: Alphabetical order of the **String** (starting after the first space and period).
- Secondary: Numerical order of the **Number** (preceding the period) if Strings are equal.

**Expected Output (for the above example):**
```
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
```

---

## Components

### 1. FileGenerationUtil

**Purpose:**  
Generates a large test file with lines in the format `Number. String`, with configurable file size and optional source phrases for the String part.

**Command-line usage:**
```
FileGenerationUtil --output "output.txt" --size 10737418240 --source "source.txt"
```

**Arguments:**
- `--output`: Path to the file to be generated.
- `--size`: Target file size in bytes (e.g., `10737418240` for 10 GB).
- `--source`: (Optional) Path to a source file containing strings to randomly use in the generated file. If omitted, a built-in set will be used.

---

### 2. Sorter

**Purpose:**  
Sorts an input file (possibly tens or hundreds of gigabytes) based on the specified sorting rules and writes the sorted output to a new file.

**Command-line usage:**
```
Sorter --input "input.txt" --output "result.txt"
```

**Arguments:**
- `--input`: Path to the unsorted input file.
- `--output`: Path to the sorted output file.
