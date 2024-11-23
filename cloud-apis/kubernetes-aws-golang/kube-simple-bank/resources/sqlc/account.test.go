package db

import "testing"

func TestCreateAccount(t *testing.T) {
	arg := CreateAccountParams{
		Owner:    "tom",
		Balance:  100,
		Currency: "USD",
	}
	account, err := testQueries.CreateAccount(arg) // todo: check default context arg
}
