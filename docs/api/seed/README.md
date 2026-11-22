# Seed Images Explanation

The images are divided into two sets:

- `app/api/Qwizzi/seed/images/development` contains seven separate files with identical image content.
- `app/api/Qwizzi/seed/images/testing` contains files with the same names as the development set, but with different images.

The testing set provides more realistic images for testing, while the development set provides simple placeholders for frontend development.

# Seed Data Explanation

The seed data is the same for both the development and testing environments. The environment can be selected by modifying the global `.cs` configuration file. By default, it is set to development.

## Data Scenario

- There are five users in the system, two of whom are administrators (`role = 2`).
- There are four quizzes:
  - East of Eden is rejected (`status = 4`).
  - The other three quizzes are approved (`status = 3`).

### Windows PowerShell

Created by `ITGuy12`, a regular user.

- Cover image: `image-01.webp`.
- Category: IT Technology.
- Five questions without images.
- Each question has four answers without images.

### Logic Gates

Created by `ITGuy12`, a regular user.

- Cover image: `image-02.webp`.
- Category: IT Technology.
- Three questions without images.
  - Each question has three answers without images.
- One additional question: “Which image shows a logic gate diagram?”
  - The question has two image-only answers:
    - `image-03.webp` — correct.
    - `image-04.webp` — incorrect.

### East of Eden

Created by `East_Eden12`, a regular user.

- Status: Rejected.
- Cover image: `image-05.webp`.
- Category: Movies.
- Ten questions without images.
- Each question has four answers.

### Silent Hill

Created by `Admin_main`, an administrator.

- Cover image: `image-06.webp`.
- Category: Games.
- Five questions without images.
  - Each question has four answers.
- One additional question: “Which location is shown in the image?”
  - Question image: `image-07.webp`.
  - The question has three answers:
    - Hospital — correct.
    - Woodside Apartments — incorrect.
    - Lakeview Hotel — incorrect.

### Completed Quiz Attempt

`GamerBoy14` completed the Logic Gates quiz with three correct answers and one incorrect answer, achieving a result of 3/4.

## Quiz Summary

| Quiz               | Questions                                                   | Answers                                                                             | Images                                                              |
| :----------------- | :---------------------------------------------------------- | :---------------------------------------------------------------------------------- | :------------------------------------------------------------------ |
| Windows PowerShell | 5                                                           | 20                                                                                  | Cover: `image-01.webp`                                              |
| Logic Gates        | 4: three standard questions and one with image-only answers | 11: nine text-only answers and two image-only answers                               | Cover: `image-02.webp`<br>Answers: `image-03.webp`, `image-04.webp` |
| East of Eden       | 10                                                          | 40                                                                                  | Cover: `image-05.webp`                                              |
| Silent Hill        | 6: five standard questions and one with an image            | 23: twenty answers for the standard questions and three for the additional question | Cover: `image-06.webp`<br>Question: `image-07.webp`                 |
| Total              | 25                                                          | 94                                                                                  | 7 files                                                             |

## Users (admin - 2, user - 1)

| Username       | Role          | Responsibility in the Scenario                      |
| :------------- | :------------ | :-------------------------------------------------- |
| `ITGuy12`      | Regular user  | Creates Windows PowerShell and Logic Gates          |
| `East_Eden12`  | Regular user  | Creates the rejected East of Eden quiz              |
| `GamerBoy14`   | Regular user  | Completes Logic Gates with a result of 3/4          |
| `Admin_main`   | Administrator | Creates the Silent Hill quiz                        |
| `Admin_second` | Administrator | Moderates quizzes, including rejecting East of Eden |

## Images and Their Owners

| Files                           | Owner         |
| :------------------------------ | :------------ |
| `image-01.webp`–`image-04.webp` | `ITGuy12`     |
| `image-05.webp`                 | `East_Eden12` |
| `image-06.webp`–`image-07.webp` | `Admin_main`  |
