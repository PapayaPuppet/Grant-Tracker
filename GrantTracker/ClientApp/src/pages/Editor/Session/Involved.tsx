import {useEffect, useState} from 'react'
import { Form, Container, Row, Col, ListGroup, CloseButton } from 'react-bootstrap'

import { useSession } from '../index'
import Dropdown from 'components/Input/Dropdown'
import { DropdownOption } from 'Models/Session'

import api from 'utils/api'
import Select from 'react-select'

//Second Section - Instructor/Funding
////Instructor -- Searchable Dropdown
////Instructor Status -- Dropdown
////Funding Source -- Multiple Response
////Organization Type -- Dropdown
////Partnership Type -- Dropdown

export default (): JSX.Element => {
  const { reducerDispatch, dropdownData, values, orgYearGuid } = useSession()
  const [instructors, setInstructors] = useState<DropdownOption[]>([])
  document.title = `${values.guid ? 'Edit' : 'New'} Session - Involved`

  if (!values)
    return (
      <p style={{ textAlign: 'center' }}>Error in loading Session details...</p>
    )

  function handleInstructorAddition (guid: string, label: string): void {
    if (!values.instructors.find(i => i.guid === guid))
      reducerDispatch({ type: 'addInstructor', payload: { guid, label } })
  }

  function handleInstructorRemoval (guid: string): void {
    reducerDispatch({ type: 'removeInstructor', payload: guid })
  }

  useEffect(() => {
    console.log(orgYearGuid)
    api
      .get('instructor', {params: {orgYearGuid: orgYearGuid}})
      .then(res => {
        res.data = res.data.filter(item => !values.instructors.find(value => value.guid === item.guid))
        setInstructors(res.data.map(isy => ({
            guid: isy.guid,
            label: `${isy.instructor.firstName} ${isy.instructor.lastName}`
          })
        ))
      })
      .catch(err => console.warn(err))
  }, [values.instructors])

  return (
    <Container>
      <Row lg={3} className='m-3'>
        <Col>
          <Form.Group>
            <Form.Label>Partnership-Type</Form.Label>
            <Dropdown
              value={values.partnershipType}
              options={dropdownData.partnershipTypes}
              onChange={(value: string) =>
                reducerDispatch({ type: 'partnership', payload: value })
              }
              disableOverlay
            />
          </Form.Group>
        </Col>
        <Col>
          <Form.Group>
            <Form.Label>Funding-Source</Form.Label>
            <Dropdown
              value={values.fundingSource}
              options={dropdownData.fundingSources}
              onChange={(value: string) =>
                reducerDispatch({ type: 'funding', payload: value })
              }
            />
          </Form.Group>
        </Col>
        <Col>
          <Form.Group>
            <Form.Label>Organization Type</Form.Label>
            <Dropdown
              value={values.organizationType}
              options={dropdownData.organizationTypes}
              onChange={(value: string) =>
                reducerDispatch({ type: 'organization', payload: value })
              }
              disableOverlay
            />
          </Form.Group>
        </Col>
      </Row>
      <Row className='m-3 pb-5'>
        <Col>
          <Form.Group>
            <Form.Label>Instructor(s)</Form.Label>
            <Select 
              id='select-instructors'
              value={{value: '', label: ''}}
              options={instructors.map(option => ({label: option.label, value: option.guid}))}
              onChange={option => {
                const instructor = instructors.filter(i => i.guid === option.value)[0]
                handleInstructorAddition(instructor.guid, instructor.label)
              }}
            />
          </Form.Group>
        </Col>
        <Col className='position-relative' lg={8} md={12} sm={12}>
          <Form.Label>Selected Instructors</Form.Label>
          <ListGroup className='d-flex flex-row flex-wrap position-absolute float-left'>
            {values.instructors.map(i => (
              <div>
                <ListGroup.Item className='d-flex flex-row p-1'>
                  <CloseButton
                    onClick={() => handleInstructorRemoval(i.guid)}
                  />
                  <div>{i.label}</div>
                </ListGroup.Item>
              </div>
            ))}
          </ListGroup>
        </Col>
      </Row>
    </Container>
  )
}

/*

            <SearchDropdown
              values={values.instructors}
              label={'Select Instructor...'}
              onChange={handleInstructorAddition}
            />
            */