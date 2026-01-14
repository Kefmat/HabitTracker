# HabitTracker

En enkel habit tracker (MVP) laget i C# med Blazor Server, EF Core og SQLite.

## Funksjoner
- Legg til habits med poeng/level sysyem
- 7-dagers oversikt for planlegging av uken
- Streaks (daglige streaks)
- Rewards som låses opp basert på all-time poeng
- Lokal lagring med SQLite

## Teknologi / stack
- .NET (Blazor Server / Interactive Server)
- ASP.NET Core
- Entity Framework Core
- SQLite

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