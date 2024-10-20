package db

import "context"

const createAccount = `
	-- name: CreateAccount :one

	INSERT INTO 
		accounts (
			owner,
			balance,
			currency
		)
		VALUES (
			$1,
			$2,
			$3
		)
		RETURNING id, owner, balance, currency, created_at
`

type CreateAccountParams struct {
	Owner    string `json:"owner"`
	Balance  int64  `json:"balance"`
	Currency string `json:"currency"`
}

func (
	q *Queries,
) CreateAccount(
	ctx context.Context,
	arg CreateAccountParams,
) (
	Account,
	error,
) {
	row := q.db.QueryRowContext(
		ctx,
		createAccount,
		arg.Owner,
		arg.Balance,
		arg.Currency,
	)
	var i Account
	err := row.Scan(
		&i.ID,
		&i.Owner,
		&i.Balance,
		&i.Currency,
		&i.CreatedAt,
	)
	return i, err
}

const getAccount = `
	-- name: GetAccount :one

	SELECT 
		id,
		owner,
		balance,
		currency,
		created_at,
		FROM accounts
		WHERE id = $1
		LIMIT 1
`

func (
	q *Queries,
) GetAccount(
	ctx context.Context,
	id int64,
) (
	Account,
	error,
) {
	row := q.db.QueryRowContext(
		ctx,
		getAccount,
		id,
	)
	var i Account
	err := row.Scan(
		&i.ID,
		&i.Owner,
		&i.Balance,
		&i.Currency,
		&i.CreatedAt,
	)
	return i, err
}

const getAccounts = `
	-- name: GetAccounts :many

	SELECT 
		id,
		owner,
		balance,
		currency,
		created_at,
		FROM accounts
		ORDER BY id
		LIMIT $1
		OFFSET $2
`

type GetAccountsParams struct {
	Limit  int32 `json:"limit"`
	Offset int32 `json:"offset"`
}

func (
	q *Queries,
) GetAccounts(
	ctx context.Context,
	arg GetAccountsParams,
) (
	[]Account,
	error,
) {
	rows, err := q.db.QueryContext(
		ctx,
		getAccounts,
		arg.Limit,
		arg.Offset,
	)
	if err != nil {
		return nil, err
	}

	defer rows.Close()
	var items []Account
	var i Account

	for rows.Next() {
		if err := rows.Scan(
			&i.ID,
			&i.Owner,
			&i.Balance,
			&i.Currency,
			&i.CreatedAt,
		); err != nil {
			return nil, err
			// todo: consider returning remaining items instead
			// * return items, err // (for the other returns too)
		}
		items = append(items, i)
	}
	if err := rows.Close(); err != nil {
		return nil, err
	}
	if err := rows.Err(); err != nil {
		return nil, err
	}

	return items, nil
}

const updateAccount = `
	--name: UpdateAccount :exec

	UPDATE
		accounts
		SET balance = $2
		WHERE id = $1
`

type UpdateAccountParams struct {
	ID      int64 `json:"id"`
	Balance int64 `json:"balance"`
}

func (
	q *Queries,
) UpdateAccount(
	ctx context.Context,
	arg UpdateAccountParams,
) error {
	_, err := q.db.ExecContext(
		ctx,
		updateAccount,
		arg.ID,
		arg.Balance,
	)
	return err
}

const updateAndGetAccount = `
	--name: UpdateAndGetAccount :one

	UPDATE accounts
	SET balance = $2
	WHERE id = $1
`

func (
	q *Queries,
) UpdateAndGetAccount(
	ctx context.Context,
	arg UpdateAccountParams,
) (
	Account,
	error,
) {
	row := q.db.QueryRowContext( // * not ExecContext - returns row sql.Result data
		// can't be converted to row sql.Row (to call .Scan(..) after)
		ctx,
		updateAndGetAccount,
		arg.ID,
		arg.Balance,
	)
	var i Account
	err := row.Scan(
		&i.ID,
		&i.Owner,
		&i.Balance,
		&i.Currency,
		&i.CreatedAt,
	)
	return i, err
}

const deleteAccount = `
	--name: DeleteAccount :exec

	DELETE FROM accounts
	WHERE id = $1
`

func (
	q *Queries,
) DeleteAccount(
	ctx context.Context,
	id int64,
) error {
	_, err := q.db.ExecContext(
		ctx,
		deleteAccount,
		id,
	)
	return err
}

const removeAccount = `
	--name: RemoveAccount :one

	DELETE 
	FROM accounts
	WHERE id = $1
	RETURNING id, owner, balance, currency, created_at
`

func (
	q *Queries,
) RemoveAccount(
	ctx context.Context,
	id int64,
) (
	Account,
	error,
) {
	row := q.db.QueryRowContext(
		ctx,
		removeAccount,
		id,
	)
	var i Account
	err := row.Scan(
		&i.ID,
		&i.Owner,
		&i.Balance,
		&i.Currency,
		&i.CreatedAt,
	)
	return i, err
}
