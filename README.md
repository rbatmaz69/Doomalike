DOOMALIKE - Schmelzwerk
Unity 6 (6000.6.3f1), Universal Render Pipeline
Einzelabgabe, Fach Game Engines

Projekt
https://hhn365-my.sharepoint.com/:f:/g/personal/rbatmaz_stud_hs-heilbronn_de/IgCTcuKZQkDiRIHgUtx_Wf8CAWep0ECdap7h6fEkz4eQSfU?email=Florian.Domino%40hs-heilbronn.de&e=cOc6Xv

GELOESTE AUFGABENTEILE
1. Levelbau mit ProBuilder: geloest
2. Spielmechanik, Teil A "High Mobility": geloest
3. Einzel- oder Mehrspieler, Teil B "Networking": geloest


1. LEVEL "SCHMELZWERK" (ProBuilder)
Eine stillgelegte Industriehalle mit Hof, Dach und Silo, 100 x 70 m gross, Hoehen von -10 m (Gleisgraben) bis +24 m (Kamin-Krone).
Der Weg fuehrt durch 11 Stationen: Start im Hof, Container-Stapel, Tor, Steg A, Kessel mit Kletterwand, Kranbahn, Steg B, zwei Saeulen ueber dem Schmelzbecken, Dachluke, Gleisgraben mit zwei Greifpunkten, Wandsprung-Kamin und Ziel.
Aus ProBuilder-Formen gebaut (Cube, Cylinder) mit Extrude, Insert Edge Loop, Face-Bearbeitung und Loechern (z.B. Tor in der Westwand, Dachluke).
Gameplay-Elemente sind Prefabs (Jump-Pad, Greifpunkt, Checkpoint) und haben feste Signalfarben:
Gruen = Kletterwand, Orange = Jump-Pad, Cyan = Greifpunkt, Gelb = Checkpoint, Violett = Ziel, Glut = Lava.


2. TEIL A - HIGH MOBILITY (FirstPersonController, GrapplingHook, JumpPad)
- Double Jump (Leertaste, in der Luft nochmal)
- Dash (Shift, 2 Ladungen, laden sich nach)
- Grappling Hook (E): Greifpunkte im Umkreis von 20 m, Seil als LineRenderer, Zug zum Punkt mit Schwung am Ende
- Jump-Pads ("Trampoline"): J1 und J2 katapultieren auf eine einstellbare Hoehe
- Wand: an Kletterwaenden festhalten, mit W klettern (begrenzte Ausdauer), mit Leertaste von der Wand abspringen


3. TEIL B - NETWORKING (Unity Networking: Netcode for GameObjects)
- Verbindung ueber Unity Relay: Ein Spieler startet "Host", gibt den 6-stelligen Join-Code weiter, andere treten damit bei. Funktioniert auch zwischen verschiedenen Netzwerken (getestet mit Windows-Build und Editor).
- Spieler erscheinen fuer die anderen als Kapsel-Koerper, jeder steuert nur seinen eigenen Spieler.
- Schiessen mit Linksklick: Treffer-Pruefung per Strahl, 25 Schaden pro Treffer, 100 Leben, eine Kugel fliegt zum Treffpunkt.
- Ausscheiden und Respawn: Bei 0 Leben wird der Spieler am letzten Checkpoint wiederbelebt, der Schuetze bekommt einen Treffer-Punkt.
- Server-seitig verwaltet (NetworkVariable fuer Leben und Treffer).
Hinweis zum Testen: Relay benoetigt eine Internetverbindung. Alternativ zwei Spieler im Editor ueber Window > Multiplayer > Multiplayer Play Mode starten (Player 2 anhaken) und den Join-Code im zweiten Fenster eingeben.


BESONDERHEITEN
- Checkpoints (CP1-CP4), KillZones ueber Schmelzbecken, Gleisgraben und als Auffangzone, Ziel mit Zeitanzeige (R = Neustart).
- Kleine Anzeige fuer Leben, Treffer und Fadenkreuz sowie eine Tastenlegende unten rechts.
- Szene: Assets/Scenes/SC_Schmelzwerk


STEUERUNG
WASD         Laufen
Leertaste    Springen (in der Luft: Double Jump)
Shift        Dash
E            Enterhaken (am Greifpunkt anzielen)
Linksklick   Schiessen
An der Wand  W klettern, Leertaste Wandsprung
Esc          Maus freigeben
R            Neustart nach dem Ziel


VERWENDETE ASSETS UND PAKETE
Keine externen Assets, keine fremden Modelle oder Texturen. Alles ist mit ProBuilder gebaut, Materialien sind einfarbige URP-Lit-Materialien.
Verwendete Unity-Pakete:
- ProBuilder
- Input System
- Universal Render Pipeline
- Netcode for GameObjects
- Multiplayer Play Mode
- Unity Relay und Unity Authentication (Unity Gaming Services)
