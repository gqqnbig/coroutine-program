package main

import "fmt"
import "time"

var c chan time.Time = make(chan time.Time)

func main() {
    time.Sleep(1 * time.Second)

    go func(){c <- time.Now()}()
    go main()
    fmt.Println(<-c)

}
