package main

import (
	"fmt"
)

var ch chan int = make(chan int)

func evaluateArg() int {
	// go func() { ch <- 1 }()

	ch <- 1
	return 2
}

func worker(n int) {
	fmt.Println(<-ch)
}

func main() {
	go worker(evaluateArg())

}
