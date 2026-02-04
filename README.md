# HabitTracker

<img width="1913" height="643" alt="7c9900b87d73f55f9f086154a2b19048" src="https://github.com/user-attachments/assets/40fe9992-5c6e-4db2-b3ef-d3368931816d" />

En enkel habit tracker (MVP) laget i C# med Blazor Server, EF Core og SQLite.

<img width="1900" height="460" alt="df47f6c7a913ea7e8df7d42c78bdda26" src="https://github.com/user-attachments/assets/3011b1f5-49e0-4c97-928f-641d1916c5fd" />

## Funksjoner
- Legg til habits med poeng/level sysyem

- 7-dagers oversikt for planlegging av uken
- Streaks (daglige streaks)
- Rewards som låses opp basert på all-time poeng
- Lokal lagring med SQLite

<img width="864" height="266" alt="964f26ff7a50c6703930239e4e67347d" src="https://github.com/user-attachments/assets/810a3903-b3ad-4e12-ba5b-e8cc99662728" />

## Teknologi / stack
- .NET (Blazor Server / Interactive Server)
- ASP.NET Core
- Entity Framework Core
- SQLite

<img width="1908" height="653" alt="2fd16f67de20274750b04c36ef07383b" src="https://github.com/user-attachments/assets/485726e3-3f05-4d32-bac4-6fdf7dad33fc" />

## Kjør prosjektet lokalt
1. Åpne terminal i prosjektmappen
2. Kjør følgende kommandoer:

   dotnet restore  
   dotnet ef database update  
   dotnet watch run  

3. Åpne nettleser og gå til adressen som vises i terminalen

Merk:
- Databasen `habittracker.db` opprettes lokalt
- Databasen er ignorert av Git og blir ikke commitet

## Videre planer
- Bedre dashboard på forsiden
- Progress til neste reward
- Streak-milestones (3, 7, 14, 30 dager)
- Integrasjoner (f.eks. Garmin / Goodreads)
- Enkel AI-coach for motivasjon og forslag
