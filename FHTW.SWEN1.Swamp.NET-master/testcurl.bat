
rem Get all messages
curl -X GET http://localhost:12000/messages --header "Content-Type: text/plain" -d ""

rem Create messages
curl -X POST http://localhost:12000/messages --header "Content-Type: text/plain" -d "Hello World. I'm a message"
curl -X POST http://localhost:12000/messages --header "Content-Type: text/plain" -d "Hello World. I'm message number 2."
curl -X POST http://localhost:12000/messages --header "Content-Type: text/plain" -d "Hello World. I'm message number 3."

rem Show first message
curl -X GET http://localhost:12000/messages/1 --header "Content-Type: text/plain" -d ""

rem Show third message
curl -X GET http://localhost:12000/messages/1 --header "Content-Type: text/plain" -d ""

rem Update a message
curl -X POST http://localhost:12000/messages/1 --header "Content-Type: text/plain" -d "Hello World. I'm message number 1."

rem Delete a message
curl -X DELETE http://localhost:12000/messages/1 --header "Content-Type: text/plain" -d ""

