# EngrLink 🎓

**EngrLink** is a university portal system designed specifically for the **College of Engineering and Architecture**. It provides a centralized and intuitive platform for **students**, **faculty**, **accounting staff**, and the **department chairman** to manage academic and administrative workflows effectively.

> 📌 Inspired by Angeles University's **SBLIVE**, EngrLink streamlines essential academic services in a **dedicated desktop experience** — optimized for the unique needs of engineering and architecture programs.

---

## 🏩 Departments Supported

* **Computer Engineering (CPE)**
* **Civil Engineering (CE)**
* **Electronics Engineering (ECE)**
* **Architecture (ARCHI)**

---

## 👥 User Roles

### 🎓 Students

* View enrolled subjects and schedules
* Monitor grades and compute GWA
* Check financial balance and payment requirements

### 👨‍🏫 Faculty

* Manage assigned subjects
* Submit and update grades
* Track advisory sections

### 🧾 Accounting

* View and update student balances
* Verify payment completion and status

### 🧑‍💼 Department Chairman

* Oversee faculty and subject assignments
* Approve subject offerings
* Monitor overall academic standing

---

## 🚀 Features

* ✅ Offline-safe startup (fallback UI if server is unreachable)
* ✅ Supabase backend integration
* ✅ Role-based navigation and access control
* ✅ Dynamic filtering of students and subjects
* ✅ Grade input and automatic GWA computation
* ✅ Responsive UI using WinUI 3 and MVVM pattern

---

## 🛠 Technologies Used

* [.NET 6](https://dotnet.microsoft.com/) with **WinUI 3**
* [Supabase](https://supabase.com/) for backend services
* MVVM and Observable Collections
* RESTful APIs via Supabase Client SDK
* XAML for UI layout and styling
* GitLab for source control and CI/CD

---

## 📦 Getting Started

1. **Clone the repository**:

   ```bash
   git clone https://gitlab.com/batongbacalmark-group/EngrLink.git
   cd EngrLink
   ```

2. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

3. **Run the app**:

   ```bash
   dotnet run
   ```

> ⚠️ Make sure your Supabase credentials (URL + API key) are correctly configured in `App.xaml.cs`.

---

## 🧠 Project Notes

* This project is designed for local, university-based use and is not connected to AUF’s actual systems.
* Inspired by SBLIVE but custom-built from scratch for offline support and desktop-first experience.
* Architecture students can access 5th-year subjects, while other programs are limited to 4 years.

---

## 📢 Contact

Have questions, suggestions, or ideas?
Open an issue or reach out at **[batongbacalmark@gmail.com](mailto:batongbacalmark@gmail.com)** **[sarmijustine@gmail.com](mailto:sarmijustine@gmail.com)** **[pinacate.stephen@gmail.com](mailto:pinacate.stephen@gmail.com)** 

---

