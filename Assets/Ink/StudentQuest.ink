VAR Report = false
VAR greeted3 = false
VAR taskCompleted3 = false
VAR allTasksDone = false
VAR Fight = false

-> start

== start ==

{ taskCompleted3:
    // Zadanie zostało wykonane wcześniej
    Teacher: Świetnie się spisałeś z tym raportem. Dobrze Cię widzieć!

    { allTasksDone:
        Teacher: Widzę, że nie tylko mnie pomogłeś!
        Teacher: Chciałbyś może poprawkę?
        + [Pewnie]
            -> Walka
        + [Później]
            -> refuse
    }

    -> DONE
- else:
    // Zadanie NIE jest wykonane
    { greeted3 == false:
        ~ greeted3 = true
        Teacher: Witaj! Potrzebuję twojej pomocy.
        Teacher: Czy możesz dostarczyć ten raport do gabinetu dyrektora?

        + [Już mam raport]
            ~ Report = true
            -> check
        + [Jasne, wezmę go]
            -> check

    - else:
        -> check
    }
}

== check ==
{ Report:
    // Raport dostarczony
    Teacher: Wspaniale! Raport dostarczony! Dziękuję Ci bardzo za pomoc.
    ~ taskCompleted3 = true

    { allTasksDone:
        Teacher: Widzę, że nie tylko mnie pomogłeś!
        Teacher: Chciałbyś może poprawkę?
        + [Pewnie]
            -> Walka
        + [Później]
            -> refuse
    }

    -> DONE
- else:
    // Gracz nie dostarczył jeszcze raportu
    Teacher: Czy dostarczyłeś raport dyrektorowi?

    + [Zaraz go zaniosę]
        -> accept
    + [Nie dam rady teraz]
        -> refuse
}

== Walka ==
Szykuj się!
// pauza – przejście do właściwego rozpoczęcia walki
-> Walka_Set

== Walka_Set ==
Szykuj się!
~ Fight = true
-> DONE

== accept ==
Teacher: Idealnie! Dziękuję za Twój czas i chęć pomocy.
-> DONE

== refuse ==
Teacher: Rozumiem, żaden problem. Wróć, kiedy będziesz mógł/mogła.
-> DONE
