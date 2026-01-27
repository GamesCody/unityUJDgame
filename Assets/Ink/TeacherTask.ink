VAR hasReport = false
VAR greeted = false
VAR taskCompleted = false

-> start

== start ==
{ taskCompleted:
    // If task was completed previously, show a short recurring message
    Teacher: (...)
    -> END
- else:
    { greeted == false:
        ~ greeted = true
        Teacher: Hello! I need your help with something.
        Teacher: Can you deliver this report to the principal's office?
        -> check
      - else:
        -> check
    }
}

== check ==
{ hasReport:
    Teacher: Great! You delivered the report!
    Thank you so much for your help.
    ~ taskCompleted = true
    -> DONE
- else:
    Teacher: Did you deliver the report to the principal?

    +   [I'll deliver it] -> accept
    +   [I can't right now] -> refuse
}

== accept ==
Teacher: Perfect! Thank you for your time.
-> DONE

== refuse ==
Teacher: Okay, no problem. Come back when you can.
-> DONE
